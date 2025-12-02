using Apollo.AI.Config;
using Apollo.AI.Plugins;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Apollo.AI;

public class ApolloAIAgent : IApolloAIAgent
{
  private readonly GlobalChatHistory _globalChatHistory = new();
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

  public async Task<string> ChatAsync(string username, string chatMessage, CancellationToken cancellationToken = default)
  {
    try
    {
      var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
      var chatHistory = _globalChatHistory.GetChatHistoryForUser(username);
      chatHistory.AddUserMessage(chatMessage);

      var response = await chatCompletionService.GetChatMessageContentAsync(
          chatHistory,
          executionSettings: _promptExecutionSettings,
          kernel: _kernel,
          cancellationToken: cancellationToken);

      _globalChatHistory.AddAIReply(username, response);

      return response.Content ?? string.Empty;
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error during chat: " + ex.Message);
      throw;
    }
  }
}
