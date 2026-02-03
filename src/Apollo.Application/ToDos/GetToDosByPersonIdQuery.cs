using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record GetToDosByPersonIdQuery(PersonId PersonId, bool IncludeCompleted = false) : IRequest<Result<IEnumerable<ToDo>>>;

public sealed class GetToDosByPersonIdQueryHandler(IToDoStore toDoStore) : IRequestHandler<GetToDosByPersonIdQuery, Result<IEnumerable<ToDo>>>
{
  public async Task<Result<IEnumerable<ToDo>>> Handle(GetToDosByPersonIdQuery request, CancellationToken cancellationToken)
  {
    try
    {
      return await toDoStore.GetByPersonIdAsync(request.PersonId, request.IncludeCompleted, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
