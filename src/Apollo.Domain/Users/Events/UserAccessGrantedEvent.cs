using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Domain.Users.Events;

public sealed record UserAccessGrantedEvent
{
  public required UserId UserId { get; init; }
  public required DateTime GrantedOn { get; init; }
}
