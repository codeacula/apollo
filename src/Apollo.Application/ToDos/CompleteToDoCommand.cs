using Apollo.Core;
using Apollo.Core.Logging;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record CompleteToDoCommand(ToDoId ToDoId) : IRequest<Result>;

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
      var linkedReminders = await GetLinkedRemindersAsync(request.ToDoId, cancellationToken);

      var completeResult = await CompleteToDoAsync(request.ToDoId, cancellationToken);
      if (completeResult.IsFailed)
      {
        return completeResult;
      }

      var cleanupResult = await CleanupRemindersForCompletedToDoAsync(linkedReminders, request.ToDoId, cancellationToken);
      return cleanupResult.IsFailed ? cleanupResult : completeResult;
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

  private async Task<Result> CompleteToDoAsync(ToDoId toDoId, CancellationToken cancellationToken)
  {
    return await toDoStore.CompleteAsync(toDoId, cancellationToken);
  }

  private async Task<Result> CleanupRemindersForCompletedToDoAsync(
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
        // (in case it was deleted by another concurrent operation)
        var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminder.ReminderTime.Value, cancellationToken);
        if (jobResult.IsFailed)
        {
          return Result.Fail("Failed to ensure reminder job still exists.");
        }
      }
    }

    return Result.Ok();
  }
}
