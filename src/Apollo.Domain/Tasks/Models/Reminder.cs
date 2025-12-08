using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Tasks.ValueObjects;

namespace Apollo.Domain.Tasks.Models;

public record Reminder
{
  public ReminderId Id { get; init; }
  public Details Details { get; init; }
  public ReminderTime ReminderTime { get; init; }
  public AcknowledgedOn? AcknowledgedOn { get; init; }
  public CreatedOn CreatedOn { get; init; }
  public UpdatedOn UpdatedOn { get; init; }
}
