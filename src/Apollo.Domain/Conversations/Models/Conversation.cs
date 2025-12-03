using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Domain.Conversations.Models;

public record Conversation
{
  public ConversationId Id { get; init; }
  public UserId UserId { get; init; }
  public ICollection<Message> Messages { get; init; } = [];
  public CreatedOn CreatedOn { get; init; }
}
