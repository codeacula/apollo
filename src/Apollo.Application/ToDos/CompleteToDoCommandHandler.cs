using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed class CompleteToDoCommandHandler(IToDoStore toDoStore, IToDoReminderScheduler toDoReminderScheduler) : IRequestHandler<CompleteToDoCommand, Result>
{
  public async Task<Result> Handle(CompleteToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var toDoResult = await toDoStore.GetAsync(request.ToDoId, cancellationToken);
      var quartzJobId = toDoResult.IsSuccess ? toDoResult.Value.Reminders.FirstOrDefault()?.QuartzJobId : null;

      var result = await toDoStore.CompleteAsync(request.ToDoId, cancellationToken);
      if (result.IsFailed || quartzJobId is null)
      {
        return result;
      }

      var remainingResult = await toDoStore.GetToDosByQuartzJobIdAsync(quartzJobId.Value, cancellationToken);
      if (remainingResult.IsSuccess && !remainingResult.Value.Any())
      {
        _ = await toDoReminderScheduler.DeleteJobAsync(quartzJobId.Value, cancellationToken);

        var afterDeleteRemainingResult = await toDoStore.GetToDosByQuartzJobIdAsync(quartzJobId.Value, cancellationToken);
        var reminderDate = afterDeleteRemainingResult.IsSuccess
          ? afterDeleteRemainingResult.Value
              .SelectMany(t => t.Reminders)
        if (afterDeleteRemainingResult.IsFailed)
        {
          return Result.Fail(afterDeleteRemainingResult.Errors.Select(e => e.Message).FirstOrDefault() ?? "Failed to get ToDos by QuartzJobId after deleting job.");
        }
        var reminderDate = afterDeleteRemainingResult.Value.SelectMany(t => t.Reminders).FirstOrDefault()?.ReminderTime.Value;

        if (reminderDate.HasValue)
        {
          _ = await toDoReminderScheduler.GetOrCreateJobAsync(reminderDate.Value, cancellationToken);
        }
      }

      return result;
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
