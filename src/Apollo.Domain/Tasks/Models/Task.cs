using Apollo.Domain.Tasks.ValueObjects;

namespace Apollo.Domain.Tasks.Models;

public record Task()
{
  public required TaskId Id { get; init; }

  public required Reminder Reminder { get; init; }
}
