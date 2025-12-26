using Apollo.Application.ToDos.Commands;
using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos.Handlers;

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
