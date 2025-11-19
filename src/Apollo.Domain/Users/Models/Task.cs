using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Domain.Users.Models;

public record Task()
{
  public required TaskId Id { get; init; }

  public required Reminder Reminder { get; init; }
}
