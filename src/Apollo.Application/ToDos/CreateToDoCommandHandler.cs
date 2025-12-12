using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed class CreateToDoCommandHandler(IToDoStore toDoStore, IToDoReminderScheduler toDoReminderScheduler) : IRequestHandler<CreateToDoCommand, Result<ToDo>>
{
  public async Task<Result<ToDo>> Handle(CreateToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var toDoId = new ToDoId(Guid.NewGuid());
      var result = await toDoStore.CreateAsync(toDoId, request.PersonId, request.Description, cancellationToken);

      if (result.IsFailed)
      {
        return result;
      }

      if (request.ReminderDate.HasValue)
      {
        var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(request.ReminderDate.Value, cancellationToken);
        if (jobResult.IsFailed)
        {
          return Result.Fail<ToDo>($"To-Do created but failed to schedule reminder job: {string.Join(", ", jobResult.Errors.Select(e => e.Message))}");
        }

        var reminderResult = await toDoStore.SetReminderAsync(toDoId, request.ReminderDate.Value, jobResult.Value, cancellationToken);
        if (reminderResult.IsFailed)
        {
          return Result.Ok(result.Value)
          .WithError($"To-Do created but failed to set reminder: {string.Join(", ", reminderResult.Errors.Select(e => e.Message))}");
        }
      }

      return result.Value;
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
