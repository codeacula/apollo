using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record UpdateToDoCommand(
  ToDoId ToDoId,
  Description Description
) : IRequest<Result>;

public sealed class UpdateToDoCommandHandler(IToDoStore toDoStore) : IRequestHandler<UpdateToDoCommand, Result>
{
  public async Task<Result> Handle(UpdateToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      return await toDoStore.UpdateAsync(request.ToDoId, request.Description, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
