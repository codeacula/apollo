using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Conversations.Models;

public record Message
{
  public MessageId Id { get; init; }
  public ConversationId ConversationId { get; init; }
  public PersonId PersonId { get; init; }
  public Content Content { get; init; }
  public CreatedOn CreatedOn { get; init; }
  public FromUser FromUser { get; init; }
}
