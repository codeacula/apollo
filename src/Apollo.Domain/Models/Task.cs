using Apollo.Domain.ValueObjects;

namespace Apollo.Domain.Models;

public record Task()
{
  public required TaskId Id { get; init; }

  public required Reminder Reminder { get; init; }
}
