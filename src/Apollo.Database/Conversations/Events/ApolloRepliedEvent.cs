namespace Apollo.Database.Conversations.Events;

public sealed record ApolloRepliedEvent : BaseEvent
{
  public required string Message { get; init; }
}
