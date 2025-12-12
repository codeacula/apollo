using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed class CompleteToDoCommandHandler(IToDoStore toDoStore, IToDoReminderScheduler toDoReminderScheduler) : IRequestHandler<CompleteToDoCommand, Result>
{
  public async Task<Result> Handle(CompleteToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var toDoResult = await toDoStore.GetAsync(request.ToDoId, cancellationToken);
      var quartzJobId = toDoResult.IsSuccess ? toDoResult.Value.Reminders.FirstOrDefault()?.QuartzJobId : null;

      var result = await toDoStore.CompleteAsync(request.ToDoId, cancellationToken);
      if (result.IsFailed || quartzJobId is null)
      {
        return Result.Fail("Failed to complete To-Do or no associated Quartz job ID found.");
      }

      var remainingResult = await toDoStore.GetToDosByQuartzJobIdAsync(quartzJobId.Value, cancellationToken);
      if (remainingResult.IsSuccess && !remainingResult.Value.Any())
      {
        _ = await toDoReminderScheduler.DeleteJobAsync(quartzJobId.Value, cancellationToken);
      }

      return result;
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
