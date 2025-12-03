using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Domain.Users.Events;

public sealed record UserUpdatedEvent
{
  public required UserId UserId { get; init; }
  public required DisplayName DisplayName { get; init; }
  public required DateTime UpdatedOn { get; init; }
}
