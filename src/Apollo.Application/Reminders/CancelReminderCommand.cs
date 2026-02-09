using Apollo.Core;
using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.Reminders;

public sealed record CancelReminderCommand(
  PersonId PersonId,
  ReminderId ReminderId
) : IRequest<Result>;

public sealed class CancelReminderCommandHandler(
  IReminderStore reminderStore,
  IToDoReminderScheduler toDoReminderScheduler) : IRequestHandler<CancelReminderCommand, Result>
{
  public async Task<Result> Handle(CancelReminderCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var reminderResult = await GetReminderAsync(request.ReminderId, cancellationToken);
      if (reminderResult.IsFailed)
      {
        return reminderResult.ToResult();
      }

      var ownershipResult = VerifyOwnership(reminderResult.Value, request.PersonId);
      if (ownershipResult.IsFailed)
      {
        return ownershipResult;
      }

      var linkedResult = await EnsureNotLinkedToToDosAsync(request.ReminderId, cancellationToken);
      return linkedResult.IsFailed ? linkedResult : await DeleteReminderAndCleanupJobAsync(reminderResult.Value, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  private async Task<Result<Reminder>> GetReminderAsync(ReminderId reminderId, CancellationToken cancellationToken)
  {
    return await reminderStore.GetAsync(reminderId, cancellationToken);
  }

  private static Result VerifyOwnership(Reminder reminder, PersonId personId)
  {
    return reminder.PersonId != personId
      ? Result.Fail("You do not have permission to cancel this reminder.")
      : Result.Ok();
  }

  private async Task<Result> EnsureNotLinkedToToDosAsync(ReminderId reminderId, CancellationToken cancellationToken)
  {
    var linkedToDosResult = await reminderStore.GetLinkedToDoIdsAsync(reminderId, cancellationToken);
    var linkedToDos = linkedToDosResult.IsSuccess ? linkedToDosResult.Value.ToList() : [];

    return linkedToDos.Count > 0
      ? Result.Fail("This reminder is linked to a todo. Please remove the reminder from the todo instead.")
      : Result.Ok();
  }

  private async Task<Result> DeleteReminderAndCleanupJobAsync(Reminder reminder, CancellationToken cancellationToken)
  {
    var deleteResult = await reminderStore.DeleteAsync(reminder.Id, cancellationToken);
    if (deleteResult.IsFailed)
    {
      return Result.Fail($"Failed to delete reminder: {deleteResult.GetErrorMessages()}");
    }

    // Clean up the Quartz job if no other reminders share it
    if (reminder.QuartzJobId is not null)
    {
      var remainingRemindersResult = await reminderStore.GetByQuartzJobIdAsync(reminder.QuartzJobId.Value, cancellationToken);
      var remainingReminders = remainingRemindersResult.IsSuccess ? remainingRemindersResult.Value.ToList() : [];

      // Only delete the job if no other reminders are using it
      if (remainingReminders.Count == 0)
      {
        _ = await toDoReminderScheduler.DeleteJobAsync(reminder.QuartzJobId.Value, cancellationToken);
      }
    }

    return Result.Ok();
  }
}
