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

public sealed class AIRequestBuilder : IAIRequestBuilder
{
  private readonly ApolloAIConfig _config;
  private readonly List<ChatMessageDTO> _messages = [];
  private readonly Dictionary<string, object> _plugins = [];

  private string _systemPrompt = "";
  private double _temperature = 0.7;
  private bool _toolCallingEnabled = true;

  public AIRequestBuilder(ApolloAIConfig config)
  {
    _config = config;
  }

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

  public async Task<AIRequestResult> ExecuteAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var toolCalls = new List<ToolCallResult>();
      var kernel = BuildKernel(toolCalls);
      var chatService = kernel.GetRequiredService<IChatCompletionService>();
      var chatHistory = BuildChatHistory();
      var settings = BuildExecutionSettings();

      var response = await chatService.GetChatMessageContentAsync(
        chatHistory,
        executionSettings: settings,
        kernel: kernel,
        cancellationToken: cancellationToken);

      return new AIRequestResult
      {
        Success = true,
        Content = response.Content ?? "",
        ToolCalls = toolCalls
      };
    }
    catch (Exception ex)
    {
      return AIRequestResult.Failure(ex.Message);
    }
  }

  private Kernel BuildKernel(List<ToolCallResult> toolCalls)
  {
    var builder = Kernel.CreateBuilder();
    builder.Services.AddOpenAIChatCompletion(_config.ModelId, new Uri(_config.Endpoint));
    builder.Services.AddSingleton<IFunctionInvocationFilter>(new FunctionInvocationFilter(toolCalls, maxToolCalls: 5));

    var kernel = builder.Build();

    kernel.Plugins.AddFromObject(new TimePlugin(TimeProvider.System), "Time");

    foreach (var (name, plugin) in _plugins)
    {
      kernel.Plugins.AddFromObject(plugin, name);
    }

    return kernel;
  }

  private ChatHistory BuildChatHistory()
  {
    var history = new ChatHistory();

    if (!string.IsNullOrWhiteSpace(_systemPrompt))
    {
      history.AddSystemMessage(_systemPrompt);
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
        ? FunctionChoiceBehavior.Auto()
        : null
    };
  }
}
