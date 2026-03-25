using Apollo.Application.ToDos.Notifications;
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

public sealed class SetToDoEnergyCommandHandler(IToDoStore toDoStore, IMediator mediator) : IRequestHandler<SetToDoEnergyCommand, Result>
{
  public async Task<Result> Handle(SetToDoEnergyCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var ownershipResult = await VerifyOwnershipAsync(request.ToDoId, request.PersonId, cancellationToken);
      if (ownershipResult.IsFailed)
      {
        return ownershipResult;
      }

      var result = await toDoStore.UpdateEnergyAsync(request.ToDoId, request.Energy, cancellationToken);
      if (result.IsSuccess)
      {
        await mediator.Publish(new ToDoEnergyUpdatedNotification(), cancellationToken);
      }

      return result;
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
