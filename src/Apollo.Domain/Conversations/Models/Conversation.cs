using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Conversations.Models;

public sealed record Conversation
{
  public required ConversationId Id { get; init; }
  public required PersonId PersonId { get; init; }
  public ICollection<Message> Messages { get; init; } = [];
  public required CreatedOn CreatedOn { get; init; }
  public required UpdatedOn UpdatedOn { get; init; }
}
