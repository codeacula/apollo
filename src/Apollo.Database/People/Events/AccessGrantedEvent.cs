namespace Apollo.Domain.People.Events;

public sealed record AccessGrantedEvent
{
  public required Guid Id { get; init; }
  public required DateTime GrantedOn { get; init; }
}
