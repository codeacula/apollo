using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed class DeleteToDoCommandHandler(IToDoStore toDoStore) : IRequestHandler<DeleteToDoCommand, Result>
{
  public async Task<Result> Handle(DeleteToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      return await toDoStore.DeleteAsync(request.ToDoId, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
