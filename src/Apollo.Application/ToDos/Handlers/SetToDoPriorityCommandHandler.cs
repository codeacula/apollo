using Apollo.Application.ToDos.Commands;
using Apollo.Core;
using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos.Handlers;

public sealed class SetToDoPriorityCommandHandler(IToDoStore toDoStore) : IRequestHandler<SetToDoPriorityCommand, Result>
{
  public async Task<Result> Handle(SetToDoPriorityCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Verify ownership by checking if the todo exists and belongs to the person
      var todoResult = await toDoStore.GetAsync(request.ToDoId, cancellationToken);
      if (todoResult.IsFailed)
      {
        return Result.Fail("To-Do not found");
      }

      if (todoResult.Value.PersonId.Value != request.PersonId.Value)
      {
        return Result.Fail("You don't have permission to update this to-do");
      }

      return await toDoStore.UpdatePriorityAsync(request.ToDoId, request.Priority, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
