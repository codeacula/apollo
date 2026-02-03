using Apollo.Core;
using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;
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
      // Get the reminder to verify ownership and get its QuartzJobId
      var reminderResult = await reminderStore.GetAsync(request.ReminderId, cancellationToken);

      if (reminderResult.IsFailed)
      {
        return Result.Fail(reminderResult.GetErrorMessages());
      }

      var reminder = reminderResult.Value;

      // Verify the reminder belongs to the requesting person
      if (reminder.PersonId != request.PersonId)
      {
        return Result.Fail("You do not have permission to cancel this reminder.");
      }

      // Check if this reminder is linked to any ToDos
      var linkedToDosResult = await reminderStore.GetLinkedToDoIdsAsync(request.ReminderId, cancellationToken);
      var linkedToDos = linkedToDosResult.IsSuccess ? linkedToDosResult.Value.ToList() : [];

      // If the reminder is linked to ToDos, only unlink it (don't delete)
      // This is for reminders that were created via ToDos.create_todo
      if (linkedToDos.Count > 0)
      {
        return Result.Fail("This reminder is linked to a todo. Please remove the reminder from the todo instead.");
      }

      // Delete the reminder (standalone reminder created via Reminders.create_reminder)
      var deleteResult = await reminderStore.DeleteAsync(request.ReminderId, cancellationToken);
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
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
