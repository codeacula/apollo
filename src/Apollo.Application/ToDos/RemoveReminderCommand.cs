using Apollo.Core;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record RemoveReminderCommand(
  ToDoId ToDoId,
  ReminderId ReminderId
) : IRequest<Result>;

public sealed class RemoveReminderCommandHandler(
  IReminderStore reminderStore,
  IToDoReminderScheduler toDoReminderScheduler) : IRequestHandler<RemoveReminderCommand, Result>
{
  public async Task<Result> Handle(RemoveReminderCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var reminderResult = await GetReminderAsync(request.ReminderId, cancellationToken);
      if (reminderResult.IsFailed)
      {
        return reminderResult.ToResult();
      }

      var unlinkResult = await UnlinkReminderFromToDoAsync(request.ReminderId, request.ToDoId, cancellationToken);
      if (unlinkResult.IsFailed)
      {
        return unlinkResult;
      }

      await CleanupOrphanedReminderAsync(reminderResult.Value, cancellationToken);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  private async Task<Result<Reminder>> GetReminderAsync(
    ReminderId reminderId,
    CancellationToken cancellationToken)
  {
    var reminderResult = await reminderStore.GetAsync(reminderId, cancellationToken);
    return reminderResult.IsFailed
      ? Result.Fail<Reminder>(reminderResult.GetErrorMessages())
      : reminderResult;
  }

  private async Task<Result> UnlinkReminderFromToDoAsync(
    ReminderId reminderId,
    ToDoId toDoId,
    CancellationToken cancellationToken)
  {
    var unlinkResult = await reminderStore.UnlinkFromToDoAsync(reminderId, toDoId, cancellationToken);
    return unlinkResult.IsFailed
      ? Result.Fail(unlinkResult.GetErrorMessages())
      : Result.Ok();
  }

  private async Task CleanupOrphanedReminderAsync(
    Reminder reminder,
    CancellationToken cancellationToken)
  {
    // Check if other ToDos are still linked to this reminder
    var remainingLinksResult = await reminderStore.GetLinkedToDoIdsAsync(reminder.Id, cancellationToken);
    var remainingLinks = remainingLinksResult.IsSuccess ? remainingLinksResult.Value.ToList() : [];

    if (remainingLinks.Count == 0 && reminder.QuartzJobId is not null)
    {
      // No other ToDos linked, delete the reminder and its job
      _ = await toDoReminderScheduler.DeleteJobAsync(reminder.QuartzJobId.Value, cancellationToken);
      _ = await reminderStore.DeleteAsync(reminder.Id, cancellationToken);
    }
  }
}
