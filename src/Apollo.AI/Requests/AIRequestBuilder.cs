using Apollo.AI.Config;
using Apollo.AI.DTOs;
using Apollo.AI.Enums;
using Apollo.AI.Plugins;
using Apollo.AI.Prompts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Apollo.AI.Requests;

public sealed class AIRequestBuilder(ApolloAIConfig config, IPromptTemplateProcessor templateProcessor) : IAIRequestBuilder
{
  private readonly ApolloAIConfig _config = config;
  private readonly IPromptTemplateProcessor _templateProcessor = templateProcessor;
  private readonly List<ChatMessageDTO> _messages = [];
  private readonly Dictionary<string, object> _plugins = [];
  private readonly Dictionary<string, string> _templateVariables = [];

  private string _systemPrompt = "";
  private double _temperature = 0.7;
  private bool _toolCallingEnabled = true;

  public IAIRequestBuilder WithSystemPrompt(string systemPrompt)
  {
    _systemPrompt = systemPrompt;
    return this;
  }

  public IAIRequestBuilder WithMessages(IEnumerable<ChatMessageDTO> messages)
  {
    _messages.AddRange(messages);
    return this;
  }

  public IAIRequestBuilder WithMessage(ChatMessageDTO message)
  {
    _messages.Add(message);
    return this;
  }

  public IAIRequestBuilder WithTemperature(double temperature)
  {
    _temperature = temperature;
    return this;
  }

  public IAIRequestBuilder WithPlugin(string pluginName, object plugin)
  {
    _plugins[pluginName] = plugin;
    return this;
  }

  public IAIRequestBuilder WithPlugins(IDictionary<string, object> plugins)
  {
    foreach (var (name, plugin) in plugins)
    {
      _plugins[name] = plugin;
    }
    return this;
  }

  public IAIRequestBuilder WithToolCalling(bool enabled = true)
  {
    _toolCallingEnabled = enabled;
    return this;
  }

  public IAIRequestBuilder FromPromptDefinition(PromptDefinition prompt)
  {
    _systemPrompt = prompt.SystemPrompt;
    _temperature = prompt.Temperature;
    return this;
  }

  public IAIRequestBuilder WithTemplateVariables(IDictionary<string, string> variables)
  {
    foreach (var (key, value) in variables)
    {
      _templateVariables[key] = value;
    }
    return this;
  }

  public async Task<AIRequestResult> ExecuteAsync(CancellationToken cancellationToken = default)
  {
    var toolCalls = new List<ToolCallResult>();

    try
    {
      var startTime = DateTimeOffset.UtcNow;
      Console.WriteLine($"[{startTime:HH:mm:ss.fff}] AI Request Started - ToolCalling: {_toolCallingEnabled}, Temp: {_temperature}");

      var kernel = BuildKernel(toolCalls);
      var chatService = kernel.GetRequiredService<IChatCompletionService>();
      var chatHistory = BuildChatHistory();
      var settings = BuildExecutionSettings();

      var beforeLLM = DateTimeOffset.UtcNow;
      Console.WriteLine($"[{beforeLLM:HH:mm:ss.fff}] Calling LLM (setup took {(beforeLLM - startTime).TotalMilliseconds:F0}ms)");
      Console.WriteLine($"  - Messages in history: {chatHistory.Count}");
      Console.WriteLine($"  - Plugins available: {kernel.Plugins.Count}");
      Console.WriteLine($"  - Tool calling enabled: {_toolCallingEnabled}");

      var response = await chatService.GetChatMessageContentAsync(
        chatHistory,
        executionSettings: settings,
        kernel: kernel,
        cancellationToken: cancellationToken);

      var afterLLM = DateTimeOffset.UtcNow;
      Console.WriteLine($"[{afterLLM:HH:mm:ss.fff}] LLM Response Received (took {(afterLLM - beforeLLM).TotalMilliseconds:F0}ms, {toolCalls.Count} tool calls)");

      return new AIRequestResult
      {
        Success = true,
        Content = response.Content ?? "",
        ToolCalls = toolCalls
      };
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Tool calling loop terminated"))
    {
      // Infinite loop detected and stopped - this is expected, not a failure
      Console.WriteLine($"[{DateTimeOffset.UtcNow:HH:mm:ss.fff}] AI Request Terminated (infinite loop detected)");
      Console.WriteLine($"  Tool calls that succeeded before termination: {toolCalls.Count}");

      // Return success with the tools that executed before the loop
      return new AIRequestResult
      {
        Success = true,
        Content = "",
        ToolCalls = toolCalls
      };
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[{DateTimeOffset.UtcNow:HH:mm:ss.fff}] AI Request Failed: {ex.Message}");
      Console.WriteLine($"  Exception Type: {ex.GetType().Name}");
      Console.WriteLine($"  Tool calls that succeeded before failure: {toolCalls.Count}");
      if (ex.InnerException != null)
      {
        Console.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
      }
      Console.WriteLine($"  Stack Trace: {ex.StackTrace}");

      // Return the tool calls that succeeded even though the request failed
      return new AIRequestResult
      {
        Success = false,
        ErrorMessage = ex.Message,
        ToolCalls = toolCalls
      };
    }
  }

  private Kernel BuildKernel(List<ToolCallResult> toolCalls)
  {
    var builder = Kernel.CreateBuilder();
    _ = builder.Services.AddOpenAIChatCompletion(_config.ModelId, new Uri(_config.Endpoint));
    _ = builder.Services.AddSingleton<IFunctionInvocationFilter>(new FunctionInvocationFilter(toolCalls, maxToolCalls: 5));

    var kernel = builder.Build();

    _ = kernel.Plugins.AddFromObject(new TimePlugin(TimeProvider.System), "Time");

    foreach (var (name, plugin) in _plugins)
    {
      _ = kernel.Plugins.AddFromObject(plugin, name);
    }

    return kernel;
  }

  private ChatHistory BuildChatHistory()
  {
    var history = new ChatHistory();

    if (!string.IsNullOrWhiteSpace(_systemPrompt))
    {
      var processedPrompt = _templateProcessor.Process(_systemPrompt, _templateVariables);
      history.AddSystemMessage(processedPrompt);
    }

    foreach (var message in _messages.OrderBy(m => m.CreatedOn))
    {
      if (message.Role == ChatRole.User)
      {
        history.AddUserMessage(message.Content);
      }
      else if (message.Role == ChatRole.Assistant)
      {
        history.AddAssistantMessage(message.Content);
      }
    }

    return history;
  }

  private OpenAIPromptExecutionSettings BuildExecutionSettings()
  {
    return new OpenAIPromptExecutionSettings
    {
      Temperature = _temperature,
      FunctionChoiceBehavior = _toolCallingEnabled
        ? FunctionChoiceBehavior.Auto(autoInvoke: true, options: new FunctionChoiceBehaviorOptions
          {
            AllowConcurrentInvocation = false,
            AllowParallelCalls = false
          })
        : null,
      MaxTokens = 2000  // Limit response size to prevent massive requests
    };
  }
}
