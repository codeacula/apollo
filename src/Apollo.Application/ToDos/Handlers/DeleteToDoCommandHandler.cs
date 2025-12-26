using Apollo.Application.ToDos.Commands;
using Apollo.Core;
using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos.Handlers;

public sealed class DeleteToDoCommandHandler(IToDoStore toDoStore, IToDoReminderScheduler toDoReminderScheduler) : IRequestHandler<DeleteToDoCommand, Result>
{
  public async Task<Result> Handle(DeleteToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var toDoResult = await toDoStore.GetAsync(request.ToDoId, cancellationToken);
      var quartzJobId = toDoResult.IsSuccess ? toDoResult.Value.Reminders.FirstOrDefault()?.QuartzJobId : null;

      var result = await toDoStore.DeleteAsync(request.ToDoId, cancellationToken);
      if (result.IsFailed || quartzJobId is null)
      {
        return result;
      }

      _ = await toDoReminderScheduler.DeleteJobAsync(quartzJobId.Value, cancellationToken);

      var afterDeleteRemainingResult = await toDoStore.GetToDosByQuartzJobIdAsync(quartzJobId.Value, cancellationToken);
      var reminderDate = afterDeleteRemainingResult.IsSuccess
        ? afterDeleteRemainingResult.Value.SelectMany(t => t.Reminders).FirstOrDefault()?.ReminderTime.Value
        : null;

      if (reminderDate.HasValue)
      {
        var getOrCreateJobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminderDate.Value, cancellationToken);
        if (getOrCreateJobResult.IsFailed)
        {
          return Result.Fail(getOrCreateJobResult.GetErrorMessages());
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
