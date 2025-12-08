namespace Apollo.Database.Conversations.Events;

public sealed record UserSentMessageEvent
{
  public required Guid Id { get; init; }
  public required string Message { get; init; }
  public required DateTime CreatedOn { get; init; }
}
