using Apollo.Application.ToDos.Commands;
using Apollo.Core;
using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos.Handlers;

public sealed class DeleteToDoCommandHandler(
  IToDoStore toDoStore,
  IReminderStore reminderStore,
  IToDoReminderScheduler toDoReminderScheduler) : IRequestHandler<DeleteToDoCommand, Result>
{
  public async Task<Result> Handle(DeleteToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Get linked reminders before deleting the ToDo
      var linkedRemindersResult = await reminderStore.GetByToDoIdAsync(request.ToDoId, cancellationToken);
      var linkedReminders = linkedRemindersResult.IsSuccess ? linkedRemindersResult.Value.ToList() : [];

      var result = await toDoStore.DeleteAsync(request.ToDoId, cancellationToken);
      if (result.IsFailed)
      {
        return result;
      }

      // Unlink reminders from the deleted ToDo and clean up if no other ToDos are linked
      foreach (var reminder in linkedReminders)
      {
        if (reminder.QuartzJobId is null)
        {
          continue;
        }

        // Unlink the reminder from this ToDo
        _ = await reminderStore.UnlinkFromToDoAsync(reminder.Id, request.ToDoId, cancellationToken);

        // Check if other ToDos are still linked to this reminder
        var remainingLinksResult = await reminderStore.GetLinkedToDoIdsAsync(reminder.Id, cancellationToken);
        var remainingLinks = remainingLinksResult.IsSuccess ? remainingLinksResult.Value.ToList() : [];

        if (remainingLinks.Count == 0)
        {
          // No other ToDos linked, delete the reminder and its job
          _ = await toDoReminderScheduler.DeleteJobAsync(reminder.QuartzJobId.Value, cancellationToken);
          _ = await reminderStore.DeleteAsync(reminder.Id, cancellationToken);
        }
        else
        {
          // Other ToDos still linked - check if we need to recreate the job
          var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminder.ReminderTime.Value, cancellationToken);
          if (jobResult.IsFailed)
          {
            return Result.Fail(jobResult.GetErrorMessages());
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
