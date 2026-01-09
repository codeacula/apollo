using Apollo.Domain.Common.Enums;

namespace Apollo.Database.Conversations.Events;

public sealed record ConversationStartedEvent
{
  public required Guid Id { get; init; }
  public required Platform PersonPlatform { get; init; }
  public required string PersonProviderId { get; init; }
  public required DateTime CreatedOn { get; init; }
}
