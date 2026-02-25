using Apollo.AI.Config;
using Apollo.AI.DTOs;
using Apollo.AI.Enums;
using Apollo.AI.Plugins;
using Apollo.AI.Prompts;
using Apollo.Core.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Apollo.AI.Requests;

public sealed class AIRequestBuilder(ApolloAIConfig config, IPromptTemplateProcessor templateProcessor, ILogger<AIRequestBuilder> logger) : IAIRequestBuilder
{
  private readonly ApolloAIConfig _config = config;
  private readonly IPromptTemplateProcessor _templateProcessor = templateProcessor;
  private readonly ILogger<AIRequestBuilder> _logger = logger;
  private readonly List<ChatMessageDTO> _messages = [];
  private readonly Dictionary<string, object> _plugins = [];
  private readonly Dictionary<string, string> _templateVariables = [];

  private string _systemPrompt = "";
  private double _temperature = 0.7;
  private bool _toolCallingEnabled = true;
  private bool _jsonModeEnabled;

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

  public IAIRequestBuilder WithJsonMode(bool enabled = true)
  {
    _jsonModeEnabled = enabled;
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
      AILogs.AIRequestStarted(_logger, _toolCallingEnabled, _temperature);

      var kernel = BuildKernel(toolCalls);
      var chatService = kernel.GetRequiredService<IChatCompletionService>();
      var chatHistory = BuildChatHistory();
      var settings = BuildExecutionSettings();

      var beforeLLM = DateTimeOffset.UtcNow;
      AILogs.LLMCallStarted(_logger, (beforeLLM - startTime).TotalMilliseconds, chatHistory.Count, kernel.Plugins.Count, _toolCallingEnabled);

      var response = await chatService.GetChatMessageContentAsync(
        chatHistory,
        executionSettings: settings,
        kernel: kernel,
        cancellationToken: cancellationToken);

      var afterLLM = DateTimeOffset.UtcNow;
      AILogs.LLMResponseReceived(_logger, (afterLLM - beforeLLM).TotalMilliseconds, toolCalls.Count);

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
      AILogs.AIRequestTerminatedLoopDetected(_logger, toolCalls.Count);

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
      AILogs.AIRequestFailed(_logger, ex.GetType().Name, toolCalls.Count, ex);

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
    _ = builder.Services.AddSingleton<IFunctionInvocationFilter>(new FunctionInvocationFilter(toolCalls, maxToolCalls: 5, _logger));

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
      switch (message.Role)
      {
        case ChatRole.User:
          history.AddUserMessage(message.Content);
          break;
        case ChatRole.Assistant:
          history.AddAssistantMessage(message.Content);
          break;
        case ChatRole.System:
        case ChatRole.Function:
          break;
      }
    }

    return history;
  }

  private OpenAIPromptExecutionSettings BuildExecutionSettings()
  {
    var settings = new OpenAIPromptExecutionSettings
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

    if (_jsonModeEnabled)
    {
      settings.ResponseFormat = "json_object";
    }

    return settings;
  }
}
