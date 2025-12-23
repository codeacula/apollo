using Apollo.Application.ToDos.Commands;
using Apollo.Core.ToDos;
using FluentResults;

namespace Apollo.Application.ToDos.Handlers;

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

      // Delete the job first, then check if any ToDos remain that need it recreated.
      // This eliminates the race condition where a ToDo could be created between
      // checking for remaining ToDos and deleting the job.
      _ = await toDoReminderScheduler.DeleteJobAsync(quartzJobId.Value, cancellationToken);

      var afterDeleteRemainingResult = await toDoStore.GetToDosByQuartzJobIdAsync(quartzJobId.Value, cancellationToken);

      if (afterDeleteRemainingResult.IsFailed)
      {
        return Result.Fail(afterDeleteRemainingResult.Errors.Select(e => e.Message).FirstOrDefault() ?? "Failed to get ToDos by QuartzJobId after deleting job.");
      }

      var reminderDate = afterDeleteRemainingResult.Value.SelectMany(t => t.Reminders).FirstOrDefault()?.ReminderTime.Value;

      if (reminderDate.HasValue)
      {
        var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminderDate.Value, cancellationToken);
        if (jobResult.IsFailed)
        {
          return Result.Fail("Failed to get or create reminder job.");
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
