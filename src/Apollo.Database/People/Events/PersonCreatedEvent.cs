using Apollo.Domain.Common;

namespace Apollo.Database.People.Events;

public sealed record PersonCreatedEvent
{
  public required Guid UserId { get; init; }
  public required string Username { get; init; }
  public required Platform Platform { get; init; }
  public required DateTime CreatedOn { get; init; }
}
