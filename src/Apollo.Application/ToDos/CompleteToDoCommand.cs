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

      var unlinkResult = await reminderStore.UnlinkFromToDoAsync(reminder.Id, toDoId, cancellationToken);
      if (unlinkResult.IsFailed)
      {
        ToDoLogs.LogFailedToUnlinkReminder(logger, reminder.Id.Value, toDoId.Value, string.Join(", ", unlinkResult.GetErrorMessages()));
      }

      var remainingLinksResult = await reminderStore.GetLinkedToDoIdsAsync(reminder.Id, cancellationToken);
      var remainingLinks = remainingLinksResult switch
      {
        { IsSuccess: true, Value: var links } => links.ToList(),
        _ => []
      };

      var cleanupResult = remainingLinks.Count == 0
        ? await DeleteReminderAndItsJobAsync(reminder, cancellationToken)
        : await RecreateReminderJobIfNeededAsync(reminder, cancellationToken);

      if (cleanupResult.IsFailed)
      {
        return cleanupResult;
      }
    }

    return Result.Ok();
  }

  private async Task<Result> DeleteReminderAndItsJobAsync(Reminder reminder, CancellationToken cancellationToken)
  {
    var deleteJobResult = await toDoReminderScheduler.DeleteJobAsync(reminder.QuartzJobId!.Value, cancellationToken);
    if (deleteJobResult.IsFailed)
    {
      ToDoLogs.LogFailedToDeleteReminderJob(logger, reminder.QuartzJobId.Value.Value, string.Join(", ", deleteJobResult.GetErrorMessages()));
    }

    var deleteReminderResult = await reminderStore.DeleteAsync(reminder.Id, cancellationToken);
    if (deleteReminderResult.IsFailed)
    {
      ToDoLogs.LogFailedToDeleteReminder(logger, reminder.Id.Value, string.Join(", ", deleteReminderResult.GetErrorMessages()));
      return deleteReminderResult;
    }

    return Result.Ok();
  }

  private async Task<Result> RecreateReminderJobIfNeededAsync(Reminder reminder, CancellationToken cancellationToken)
  {
    var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminder.ReminderTime.Value, cancellationToken);
    return jobResult.IsFailed ? Result.Fail("Failed to ensure reminder job still exists.") : Result.Ok();
  }
}
