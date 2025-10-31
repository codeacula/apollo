using Apollo.AI.Config;
using Apollo.AI.Plugins;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Apollo.AI;

public class ApolloAIAgent : IApolloAIAgent
{
  private readonly ChatHistory _chatHistory = [];
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
    _ = _kernel.Plugins.AddFromType<TimePlugin>("Time");
  }

  public async Task<string> ChatAsync(string chatMessage)
  {
    try
    {
      _chatHistory.AddUserMessage(chatMessage);

      var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

      var result = await chatCompletionService.GetChatMessageContentAsync(
          _chatHistory,
          executionSettings: _promptExecutionSettings,
          kernel: _kernel);

      Console.WriteLine("Assistant > " + result);
      _chatHistory.AddMessage(result.Role, result.Content ?? string.Empty);

      return result.Content ?? "Nuffin";
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error during chat: " + ex.Message);
      throw;
    }
  }
}
