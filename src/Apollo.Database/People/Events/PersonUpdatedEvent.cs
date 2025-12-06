using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.People.Events;

public sealed record PersonUpdatedEvent
{
  public required PersonId UserId { get; init; }
  public required DisplayName DisplayName { get; init; }
  public required DateTime UpdatedOn { get; init; }
}
