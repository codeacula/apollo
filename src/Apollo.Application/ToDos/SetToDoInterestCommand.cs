using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record SetToDoInterestCommand(
  PersonId PersonId,
  ToDoId ToDoId,
  Interest Interest
) : IRequest<Result>;

public sealed class SetToDoInterestCommandHandler(IToDoStore toDoStore) : IRequestHandler<SetToDoInterestCommand, Result>
{
  public async Task<Result> Handle(SetToDoInterestCommand request, CancellationToken cancellationToken)
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
        return await toDoStore.UpdateInterestAsync(request.ToDoId, request.Interest, cancellationToken);
      }
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
