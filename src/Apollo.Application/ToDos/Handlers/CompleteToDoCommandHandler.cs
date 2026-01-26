using Apollo.Application.ToDos.Commands;
using Apollo.Core;
using Apollo.Core.Logging;
using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos.Handlers;

public sealed class CompleteToDoCommandHandler(
  IToDoStore toDoStore,
  IReminderStore reminderStore,
  IToDoReminderScheduler toDoReminderScheduler,
  ILogger<CompleteToDoCommandHandler> logger) : IRequestHandler<CompleteToDoCommand, Result>
{
  public async Task<Result> Handle(CompleteToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Get linked reminders before completing the ToDo
      var linkedRemindersResult = await reminderStore.GetByToDoIdAsync(request.ToDoId, cancellationToken);
      var linkedReminders = linkedRemindersResult.IsSuccess ? linkedRemindersResult.Value.ToList() : [];

      var result = await toDoStore.CompleteAsync(request.ToDoId, cancellationToken);
      if (result.IsFailed)
      {
        return result;
      }

      // Unlink reminders from the completed ToDo and clean up if no other ToDos are linked
      foreach (var reminder in linkedReminders)
      {
        if (reminder.QuartzJobId is null)
        {
          continue;
        }

        // Unlink the reminder from this ToDo
        var unlinkResult = await reminderStore.UnlinkFromToDoAsync(reminder.Id, request.ToDoId, cancellationToken);
        if (unlinkResult.IsFailed)
        {
          ToDoLogs.LogFailedToUnlinkReminder(logger, reminder.Id.Value, request.ToDoId.Value, string.Join(", ", unlinkResult.GetErrorMessages()));
        }

        // Check if other ToDos are still linked to this reminder
        var remainingLinksResult = await reminderStore.GetLinkedToDoIdsAsync(reminder.Id, cancellationToken);
        var remainingLinks = remainingLinksResult.IsSuccess ? remainingLinksResult.Value.ToList() : [];

        if (remainingLinks.Count == 0)
        {
          // No other ToDos linked, delete the reminder and its job
          var deleteJobResult = await toDoReminderScheduler.DeleteJobAsync(reminder.QuartzJobId.Value, cancellationToken);
          if (deleteJobResult.IsFailed)
          {
            ToDoLogs.LogFailedToDeleteReminderJob(logger, reminder.QuartzJobId.Value.Value, string.Join(", ", deleteJobResult.GetErrorMessages()));
          }

          var deleteReminderResult = await reminderStore.DeleteAsync(reminder.Id, cancellationToken);
          if (deleteReminderResult.IsFailed)
          {
            ToDoLogs.LogFailedToDeleteReminder(logger, reminder.Id.Value, string.Join(", ", deleteReminderResult.GetErrorMessages()));
          }
        }
        else
        {
          // Other ToDos still linked - check if we need to recreate the job
          // (in case it was deleted by another concurrent operation)
          var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminder.ReminderTime.Value, cancellationToken);
          if (jobResult.IsFailed)
          {
            return Result.Fail("Failed to ensure reminder job still exists.");
          }
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
