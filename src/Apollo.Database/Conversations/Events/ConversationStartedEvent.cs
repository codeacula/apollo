namespace Apollo.Database.Conversations.Events;

public sealed record ConversationStartedEvent
{
  public required Guid Id { get; init; }
  public required Guid PersonId { get; init; }
  public required DateTime CreatedOn { get; init; }
}
