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
      var ownershipResult = await VerifyOwnershipAsync(request.ToDoId, request.PersonId, cancellationToken);
      return ownershipResult.IsFailed
        ? ownershipResult
        : await toDoStore.UpdateInterestAsync(request.ToDoId, request.Interest, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  private async Task<Result> VerifyOwnershipAsync(ToDoId toDoId, PersonId personId, CancellationToken cancellationToken)
  {
    var todoResult = await toDoStore.GetAsync(toDoId, cancellationToken);

    return todoResult switch
    {
      { IsFailed: true } => Result.Fail("To-Do not found"),
      _ when todoResult.Value.PersonId.Value != personId.Value => Result.Fail("You don't have permission to update this to-do"),
      _ => Result.Ok()
    };
  }
}
