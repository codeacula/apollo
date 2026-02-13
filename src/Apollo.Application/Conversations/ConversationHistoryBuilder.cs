using Apollo.AI.DTOs;
using Apollo.AI.Enums;
using Apollo.Domain.Conversations.Models;

namespace Apollo.Application.Conversations;

public static class ConversationHistoryBuilder
{
  private const int DefaultMaxMessages = 15;
  private const int DefaultMinMessages = 6;

  public static List<ChatMessageDTO> BuildForToolPlanning(
    Conversation conversation,
    IReadOnlyCollection<string> activeTodoIds,
    int maxMessages = DefaultMaxMessages,
    int minMessages = DefaultMinMessages)
  {
    var orderedMessages = conversation.Messages
      .OrderBy(m => m.CreatedOn.Value)
      .ToList();

    if (orderedMessages.Count <= minMessages)
    {
      return ToChatMessages(orderedMessages);
    }

    var recentMessages = orderedMessages.TakeLast(minMessages);
    var contextualMessages = orderedMessages.Where(m => IsContextRelevant(m.Content.Value, activeTodoIds));

    var selected = recentMessages
      .Concat(contextualMessages)
      .DistinctBy(m => m.Id.Value)
      .OrderBy(m => m.CreatedOn.Value)
      .ToList();

    if (selected.Count > maxMessages)
    {
      selected = [.. selected.TakeLast(maxMessages)];
    }

    return ToChatMessages(selected);
  }

  public static List<ChatMessageDTO> BuildForResponse(Conversation conversation)
  {
    var orderedMessages = conversation.Messages
      .OrderBy(m => m.CreatedOn.Value)
      .ToList();

    return ToChatMessages(orderedMessages);
  }

  private static bool IsContextRelevant(string content, IReadOnlyCollection<string> activeTodoIds)
  {
    return ContainsTimezoneKeyword(content) || activeTodoIds.Any(id => content.Contains(id, StringComparison.OrdinalIgnoreCase));
  }

  private static bool ContainsTimezoneKeyword(string content)
  {
    return content.Contains("timezone", StringComparison.OrdinalIgnoreCase)
      || content.Contains("time zone", StringComparison.OrdinalIgnoreCase)
      || content.Contains("tz", StringComparison.OrdinalIgnoreCase);
  }

  private static List<ChatMessageDTO> ToChatMessages(IEnumerable<Message> messages)
  {
    return [.. messages
      .Select(m => new ChatMessageDTO(
        m.FromUser.Value ? ChatRole.User : ChatRole.Assistant,
        m.Content.Value,
        m.CreatedOn.Value))];
  }
}
