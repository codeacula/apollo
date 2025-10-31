using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Apollo.AI;

public class GlobalChatHistory
{
  public Dictionary<string, ChatHistory> ChatHistories { get; } = [];

  public ChatHistory GetChatHistoryForUser(string userName)
  {
    if (!ChatHistories.TryGetValue(userName, out ChatHistory? value))
    {
      value = [];
      ChatHistories[userName] = value;
    }
    return value;
  }

  public void AddAIReply(string userName, ChatMessageContent result)
  {
    var chatHistory = GetChatHistoryForUser(userName);
    chatHistory.AddMessage(result.Role, result.Content ?? string.Empty);
  }

  public void AddUserChatMessage(string userName, string message)
  {
    var chatHistory = GetChatHistoryForUser(userName);
    chatHistory.AddUserMessage(message);
  }
}
