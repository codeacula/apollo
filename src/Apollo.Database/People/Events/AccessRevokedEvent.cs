namespace Apollo.Domain.People.Events;

public sealed record AccessRevokedEvent
{
  public required Guid UserId { get; init; }
  public required DateTime RevokedOn { get; init; }
}
