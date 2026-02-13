using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

namespace Apollo.Domain.ToDos.Models;

public sealed record Reminder
{
  public required ReminderId Id { get; init; }
  public required PersonId PersonId { get; init; }
  public required Details Details { get; init; }
  public QuartzJobId? QuartzJobId { get; init; }
  public required ReminderTime ReminderTime { get; init; }
  public AcknowledgedOn? AcknowledgedOn { get; init; }
  public required CreatedOn CreatedOn { get; init; }
  public required UpdatedOn UpdatedOn { get; init; }
}
