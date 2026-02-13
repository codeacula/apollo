using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Conversations.Models;

public sealed record Message
{
  public required MessageId Id { get; init; }
  public required ConversationId ConversationId { get; init; }
  public required PersonId PersonId { get; init; }
  public required Content Content { get; init; }
  public required CreatedOn CreatedOn { get; init; }
  public required FromUser FromUser { get; init; }
}
