using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Domain.Users.Models;

public record Reminder()
{
  public required ReminderId Id { get; init; }

  public required QuartzJobId QuartzJobId { get; init; }

  public required ReminderTime ReminderTime { get; init; }
}
