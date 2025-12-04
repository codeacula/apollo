using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Domain.Users.Events;

public sealed record UserCreatedEvent
{
  public required UserId UserId { get; init; }
  public required Username Username { get; init; }
  public required CreatedOn CreatedOn { get; init; }
}
