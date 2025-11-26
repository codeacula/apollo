using Apollo.Domain.ValueObjects;

namespace Apollo.Domain.Models;

public record Reminder()
{
  public required ReminderId Id { get; init; }

  public required QuartzJobId QuartzJobId { get; init; }

  public required ReminderTime ReminderTime { get; init; }
}
