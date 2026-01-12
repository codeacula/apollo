using Apollo.AI.Config;
using Apollo.AI.DTOs;
using Apollo.AI.Enums;
using Apollo.AI.Plugins;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Apollo.AI;

public class ApolloAIAgent : IApolloAIAgent
{
  private readonly Kernel _kernel;
  private readonly OpenAIPromptExecutionSettings _promptExecutionSettings = new()
  {
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
  };

  public ApolloAIAgent(ApolloAIConfig apolloAIConfig)
  {
    var builder = Kernel.CreateBuilder();
    _ = builder.Services.AddOpenAIChatCompletion(apolloAIConfig.ModelId, new Uri(apolloAIConfig.Endpoint));
    _kernel = builder.Build();
    AddPlugin(new TimePlugin(TimeProvider.System), "Time");
  }

  public void AddPlugin(object plugin, string pluginName)
  {
    _ = _kernel.Plugins.AddFromObject(plugin, pluginName);
  }

  public void AddPlugin<TPluginType>(string pluginName)
  {
    _ = _kernel.Plugins.AddFromType<TPluginType>(pluginName);
  }

  public async Task<string> ChatAsync(ChatCompletionRequestDTO chatCompletionRequest, CancellationToken cancellationToken = default)
  {
    try
    {
      var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
      ChatHistory chatHistory = [];

      chatHistory.AddSystemMessage(chatCompletionRequest.SystemMessage);

      chatCompletionRequest.Messages.OrderBy(msg => msg.CreatedOn).ToList().ForEach(msg =>
      {
        if (msg.Role == ChatRole.User)
        {
          chatHistory.AddUserMessage(msg.Content);
        }
        else if (msg.Role == ChatRole.Assistant)
        {
          chatHistory.AddAssistantMessage(msg.Content);
        }
      });

      var response = await chatCompletionService.GetChatMessageContentAsync(
          chatHistory,
          executionSettings: _promptExecutionSettings,
          kernel: _kernel,
          cancellationToken: cancellationToken);

      return response == null || response.Content == null
        ? throw new InvalidOperationException("Received null response from chat completion service.")
        : response.Content;
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error during chat: " + ex.Message);
      throw;
    }
  }

  public async Task<TwoPhaseCompletionResultDTO> TwoPhaseCompletionAsync(
    TwoPhaseCompletionRequestDTO request,
    double toolCallingTemperature,
    double responseTemperature,
    CancellationToken cancellationToken = default)
  {
    var actionsTaken = new List<string>();

    // Phase 1: Tool calling with low temperature (no conversation history, just current context)
    var toolCallingResult = await ExecuteToolCallingPhaseAsync(
      request.ToolCallingSystemPrompt,
      request.UserMessage,
      request.CurrentToDosContext,
      toolCallingTemperature,
      actionsTaken,
      cancellationToken);

    // Phase 2: Response generation with high temperature (includes conversation history)
    var response = await ExecuteResponsePhaseAsync(
      request.ResponseSystemPrompt,
      request.UserMessage,
      request.ConversationHistory,
      actionsTaken,
      responseTemperature,
      cancellationToken);

    return new TwoPhaseCompletionResultDTO(response, actionsTaken);
  }

  private async Task<string> ExecuteToolCallingPhaseAsync(
    string systemPrompt,
    string userMessage,
    string? currentToDosContext,
    double temperature,
    List<string> actionsTaken,
    CancellationToken cancellationToken)
  {
    try
    {
      var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
      ChatHistory chatHistory = [];

      var fullSystemPrompt = systemPrompt;
      if (!string.IsNullOrWhiteSpace(currentToDosContext))
      {
        fullSystemPrompt += $"\n\n# Current Active ToDos\n{currentToDosContext}";
      }

      chatHistory.AddSystemMessage(fullSystemPrompt);
      chatHistory.AddUserMessage(userMessage);

      var toolCallingSettings = new OpenAIPromptExecutionSettings
      {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        Temperature = temperature
      };

      // Create a wrapper to capture function results
      var originalPlugins = _kernel.Plugins.ToList();
      var functionInvocationFilter = new ActionTrackingFilter(actionsTaken);
      _kernel.FunctionInvocationFilters.Add(functionInvocationFilter);

      try
      {
        var response = await chatCompletionService.GetChatMessageContentAsync(
          chatHistory,
          executionSettings: toolCallingSettings,
          kernel: _kernel,
          cancellationToken: cancellationToken);

        return response?.Content ?? string.Empty;
      }
      finally
      {
        _kernel.FunctionInvocationFilters.Remove(functionInvocationFilter);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error during tool calling phase: {ex.Message}");
      throw;
    }
  }

  private async Task<string> ExecuteResponsePhaseAsync(
    string systemPrompt,
    string userMessage,
    List<ChatMessageDTO> conversationHistory,
    List<string> actionsTaken,
    double temperature,
    CancellationToken cancellationToken)
  {
    try
    {
      var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
      ChatHistory chatHistory = [];

      var fullSystemPrompt = systemPrompt;
      if (actionsTaken.Count > 0)
      {
        fullSystemPrompt += $"\n\n# Actions Taken\n{string.Join("\n", actionsTaken)}";
      }

      chatHistory.AddSystemMessage(fullSystemPrompt);

      // Add conversation history
      conversationHistory.OrderBy(msg => msg.CreatedOn).ToList().ForEach(msg =>
      {
        if (msg.Role == ChatRole.User)
        {
          chatHistory.AddUserMessage(msg.Content);
        }
        else if (msg.Role == ChatRole.Assistant)
        {
          chatHistory.AddAssistantMessage(msg.Content);
        }
      });

      // Add current message
      chatHistory.AddUserMessage(userMessage);

      var responseSettings = new OpenAIPromptExecutionSettings
      {
        FunctionChoiceBehavior = FunctionChoiceBehavior.None(),
        Temperature = temperature
      };

      var response = await chatCompletionService.GetChatMessageContentAsync(
        chatHistory,
        executionSettings: responseSettings,
        kernel: _kernel,
        cancellationToken: cancellationToken);

      return response == null || response.Content == null
        ? throw new InvalidOperationException("Received null response from chat completion service.")
        : response.Content;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error during response phase: {ex.Message}");
      throw;
    }
  }

  private sealed class ActionTrackingFilter(List<string> actionsTaken) : IFunctionInvocationFilter
  {
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
      await next(context);

      // Track the action after it completes
      var functionName = $"{context.Function.PluginName}-{context.Function.Name}";
      var result = context.Result?.GetValue<string>() ?? "completed";

      // Only track successful actions (not Time plugin calls)
      if (context.Function.PluginName != "Time")
      {
        actionsTaken.Add($"{functionName}: {result}");
      }
    }
  }
}
