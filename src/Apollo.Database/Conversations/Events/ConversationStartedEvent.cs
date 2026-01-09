namespace Apollo.Database.Conversations.Events;

public sealed record ConversationStartedEvent : BaseEvent
{
  public required Guid PersonId { get; init; }
}
