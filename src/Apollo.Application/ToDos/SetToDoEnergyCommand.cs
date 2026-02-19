using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record SetToDoEnergyCommand(
  PersonId PersonId,
  ToDoId ToDoId,
  Energy Energy
) : IRequest<Result>;

public sealed class SetToDoEnergyCommandHandler(IToDoStore toDoStore) : IRequestHandler<SetToDoEnergyCommand, Result>
{
  public async Task<Result> Handle(SetToDoEnergyCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var ownershipResult = await VerifyOwnershipAsync(request.ToDoId, request.PersonId, cancellationToken);
      return ownershipResult.IsFailed
        ? ownershipResult
        : await toDoStore.UpdateEnergyAsync(request.ToDoId, request.Energy, cancellationToken);
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
