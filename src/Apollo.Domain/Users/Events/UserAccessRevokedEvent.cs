using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Domain.Users.Events;

public sealed record UserAccessRevokedEvent
{
  public required UserId UserId { get; init; }
  public required DateTime RevokedOn { get; init; }
}
