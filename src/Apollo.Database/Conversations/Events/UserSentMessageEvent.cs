namespace Apollo.Database.Conversations.Events;

public sealed record UserSentMessageEvent : BaseEvent
{
  public required string Message { get; init; }
}
