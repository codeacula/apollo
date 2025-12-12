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

      chatCompletionRequest.Messages.ForEach(msg =>
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
}
