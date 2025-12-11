using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Core.ToDos;

public interface IToDoReminderScheduler
{
  Task<Result<QuartzJobId>> ScheduleReminderAsync(ToDoId toDoId, DateTime reminderTime, CancellationToken cancellationToken = default);
  Task<Result> CancelReminderAsync(QuartzJobId quartzJobId, CancellationToken cancellationToken = default);
}
