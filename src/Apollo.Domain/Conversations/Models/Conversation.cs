using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Conversations.Models;

public record Conversation
{
  public ConversationId Id { get; init; }
  public PersonId PersonId { get; init; }
  public ICollection<Message> Messages { get; init; } = [];
  public CreatedOn CreatedOn { get; init; }
  public UpdatedOn UpdatedOn { get; init; }
}
