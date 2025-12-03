using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Domain.Users.Events;

public sealed record UserCreatedEvent
{
  public required UserId UserId { get; init; }
  public required Username Username { get; init; }
  public required DisplayName DisplayName { get; init; }
  public required bool HasAccess { get; init; }
  public required DateTime CreatedOn { get; init; }
}
