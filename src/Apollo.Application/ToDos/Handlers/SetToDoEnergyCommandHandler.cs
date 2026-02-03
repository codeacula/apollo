using Apollo.Application.ToDos.Commands;
using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos.Handlers;

public sealed class SetToDoEnergyCommandHandler(IToDoStore toDoStore) : IRequestHandler<SetToDoEnergyCommand, Result>
{
  public async Task<Result> Handle(SetToDoEnergyCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Verify ownership by checking if the todo exists and belongs to the person
      var todoResult = await toDoStore.GetAsync(request.ToDoId, cancellationToken);
      if (todoResult.IsFailed)
      {
        return Result.Fail("To-Do not found");
      }
      else if (todoResult.Value.PersonId.Value != request.PersonId.Value)
      {
        return Result.Fail("You don't have permission to update this to-do");
      }
      else
      {
        return await toDoStore.UpdateEnergyAsync(request.ToDoId, request.Energy, cancellationToken);
      }
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
