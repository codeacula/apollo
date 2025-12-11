using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed class CreateToDoCommandHandler(IToDoStore toDoStore) : IRequestHandler<CreateToDoCommand, Result<ToDo>>
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
        var reminderResult = await toDoStore.SetReminderAsync(toDoId, request.ReminderDate.Value, cancellationToken);
        if (reminderResult.IsFailed)
        {
          return Result.Fail<ToDo>($"ToDo created but failed to set reminder: {string.Join(", ", reminderResult.Errors.Select(e => e.Message))}");
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
