using Apollo.Core;
using Apollo.Core.ToDos;
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
      // Get the reminder to find its QuartzJobId
      var reminderResult = await reminderStore.GetAsync(request.ReminderId, cancellationToken);

      if (reminderResult.IsFailed)
      {
        return Result.Fail(reminderResult.GetErrorMessages());
      }

      var reminder = reminderResult.Value;

      // Unlink the reminder from the ToDo
      var unlinkResult = await reminderStore.UnlinkFromToDoAsync(request.ReminderId, request.ToDoId, cancellationToken);

      if (unlinkResult.IsFailed)
      {
        return Result.Fail(unlinkResult.GetErrorMessages());
      }

      // Check if other ToDos are still linked to this reminder
      var remainingLinksResult = await reminderStore.GetLinkedToDoIdsAsync(request.ReminderId, cancellationToken);
      var remainingLinks = remainingLinksResult.IsSuccess ? remainingLinksResult.Value.ToList() : [];

      if (remainingLinks.Count == 0 && reminder.QuartzJobId is not null)
      {
        // No other ToDos linked, delete the reminder and its job
        _ = await toDoReminderScheduler.DeleteJobAsync(reminder.QuartzJobId.Value, cancellationToken);
        _ = await reminderStore.DeleteAsync(request.ReminderId, cancellationToken);
      }

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
