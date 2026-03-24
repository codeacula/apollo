using Apollo.Application.ToDos.Notifications;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record UpdateToDoCommand(
  ToDoId ToDoId,
  Description Description
) : IRequest<Result>;

public sealed class UpdateToDoCommandHandler(IToDoStore toDoStore, IMediator mediator) : IRequestHandler<UpdateToDoCommand, Result>
{
  public async Task<Result> Handle(UpdateToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var result = await toDoStore.UpdateAsync(request.ToDoId, request.Description, cancellationToken);
      if (result.IsSuccess)
      {
        await mediator.Publish(new ToDoUpdatedNotification(), cancellationToken);
      }

      return result;
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
