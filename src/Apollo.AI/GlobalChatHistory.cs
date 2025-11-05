using System.Collections.Concurrent;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Apollo.AI;

public class GlobalChatHistory
{
  public ConcurrentDictionary<string, ChatHistory> ChatHistories { get; } = new();

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
