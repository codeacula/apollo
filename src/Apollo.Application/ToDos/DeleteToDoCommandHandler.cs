using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed class DeleteToDoCommandHandler(
  IToDoStore toDoStore,
  IToDoReminderScheduler reminderScheduler) : IRequestHandler<DeleteToDoCommand, Result>
{
  public async Task<Result> Handle(DeleteToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Get the ToDo to check if it has a scheduled reminder
      var toDoResult = await toDoStore.GetAsync(request.ToDoId, cancellationToken);
      if (toDoResult.IsSuccess)
      {
        var toDo = toDoResult.Value;
        // Cancel all scheduled reminders
        foreach (var reminder in toDo.Reminders.Where(r => r.QuartzJobId.HasValue))
        {
          _ = await reminderScheduler.CancelReminderAsync(reminder.QuartzJobId!.Value, cancellationToken);
        }
      }

      return await toDoStore.DeleteAsync(request.ToDoId, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
