using Apollo.Core;
using Apollo.Core.Logging;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record DeleteToDoCommand(ToDoId ToDoId) : IRequest<Result>;

public sealed class DeleteToDoCommandHandler(
  IToDoStore toDoStore,
  IReminderStore reminderStore,
  IToDoReminderScheduler toDoReminderScheduler,
  ILogger<DeleteToDoCommandHandler> logger) : IRequestHandler<DeleteToDoCommand, Result>
{
  public async Task<Result> Handle(DeleteToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var linkedReminders = await GetLinkedRemindersAsync(request.ToDoId, cancellationToken);

      var deleteResult = await DeleteToDoAsync(request.ToDoId, cancellationToken);
      if (deleteResult.IsFailed)
      {
        return deleteResult;
      }

      var cleanupResult = await CleanupRemindersForDeletedToDoAsync(linkedReminders, request.ToDoId, cancellationToken);
      return cleanupResult.IsFailed ? cleanupResult : deleteResult;
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  private async Task<List<Reminder>> GetLinkedRemindersAsync(ToDoId toDoId, CancellationToken cancellationToken)
  {
    var linkedRemindersResult = await reminderStore.GetByToDoIdAsync(toDoId, cancellationToken);
    return linkedRemindersResult.IsSuccess ? [.. linkedRemindersResult.Value] : [];
  }

  private async Task<Result> DeleteToDoAsync(ToDoId toDoId, CancellationToken cancellationToken)
  {
    return await toDoStore.DeleteAsync(toDoId, cancellationToken);
  }

  private async Task<Result> CleanupRemindersForDeletedToDoAsync(
    List<Reminder> reminders,
    ToDoId toDoId,
    CancellationToken cancellationToken)
  {
    foreach (var reminder in reminders)
    {
      if (reminder.QuartzJobId is null)
      {
        continue;
      }

      // Unlink the reminder from this ToDo
      var unlinkResult = await reminderStore.UnlinkFromToDoAsync(reminder.Id, toDoId, cancellationToken);
      if (unlinkResult.IsFailed)
      {
        ToDoLogs.LogFailedToUnlinkReminder(logger, reminder.Id.Value, toDoId.Value, string.Join(", ", unlinkResult.GetErrorMessages()));
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
        var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminder.ReminderTime.Value, cancellationToken);
        if (jobResult.IsFailed)
        {
          return Result.Fail(jobResult.GetErrorMessages());
        }
      }
    }

    return Result.Ok();
  }
}
