using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed class CompleteToDoCommandHandler(
  IToDoStore toDoStore,
  IToDoReminderScheduler reminderScheduler) : IRequestHandler<CompleteToDoCommand, Result>
{
  public async Task<Result> Handle(CompleteToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Get the ToDo to check if it has a scheduled reminder
      var toDoResult = await toDoStore.GetAsync(request.ToDoId, cancellationToken);
      if (toDoResult.IsSuccess)
      {
        var toDo = toDoResult.Value;
        // Cancel scheduled reminder if it exists
        if (toDo.Reminders.Any(r => r.QuartzJobId.HasValue))
        {
          var reminder = toDo.Reminders.First(r => r.QuartzJobId.HasValue);
          _ = await reminderScheduler.CancelReminderAsync(reminder.QuartzJobId!.Value, cancellationToken);
        }
      }

      return await toDoStore.CompleteAsync(request.ToDoId, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
