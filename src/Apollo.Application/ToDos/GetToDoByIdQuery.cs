using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record GetToDoByIdQuery(ToDoId ToDoId) : IRequest<Result<ToDo>>;

public sealed class GetToDoByIdQueryHandler(IToDoStore toDoStore) : IRequestHandler<GetToDoByIdQuery, Result<ToDo>>
{
  public async Task<Result<ToDo>> Handle(GetToDoByIdQuery request, CancellationToken cancellationToken)
  {
    try
    {
      return await toDoStore.GetAsync(request.ToDoId, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
