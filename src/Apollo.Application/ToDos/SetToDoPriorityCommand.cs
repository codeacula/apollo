using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record SetToDoPriorityCommand(
  PersonId PersonId,
  ToDoId ToDoId,
  Priority Priority
) : IRequest<Result>;

public sealed class SetToDoPriorityCommandHandler(IToDoStore toDoStore) : IRequestHandler<SetToDoPriorityCommand, Result>
{
  public async Task<Result> Handle(SetToDoPriorityCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var ownershipResult = await VerifyOwnershipAsync(request.ToDoId, request.PersonId, cancellationToken);
      if (ownershipResult.IsFailed)
      {
        return ownershipResult;
      }

      return await toDoStore.UpdatePriorityAsync(request.ToDoId, request.Priority, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  private async Task<Result> VerifyOwnershipAsync(ToDoId toDoId, PersonId personId, CancellationToken cancellationToken)
  {
    var todoResult = await toDoStore.GetAsync(toDoId, cancellationToken);
    if (todoResult.IsFailed)
    {
      return Result.Fail("To-Do not found");
    }

    if (todoResult.Value.PersonId.Value != personId.Value)
    {
      return Result.Fail("You don't have permission to update this to-do");
    }

    return Result.Ok();
  }
}
