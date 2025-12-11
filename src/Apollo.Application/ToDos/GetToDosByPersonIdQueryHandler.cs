using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.Models;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed class GetToDosByPersonIdQueryHandler(IToDoStore toDoStore) : IRequestHandler<GetToDosByPersonIdQuery, Result<IEnumerable<ToDo>>>
{
  public async Task<Result<IEnumerable<ToDo>>> Handle(GetToDosByPersonIdQuery request, CancellationToken cancellationToken)
  {
    try
    {
      return await toDoStore.GetByPersonIdAsync(request.PersonId, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
