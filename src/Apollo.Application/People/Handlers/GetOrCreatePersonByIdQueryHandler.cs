using Apollo.Application.People.Queries;
using Apollo.Core.People;
using Apollo.Domain.People.Models;

using FluentResults;

namespace Apollo.Application.People.Handlers;

public sealed class GetOrCreatePersonByIdQueryHandler(IPersonStore personStore)
  : IRequestHandler<GetOrCreatePersonByIdQuery, Result<Person>>
{
  public async Task<Result<Person>> Handle(GetOrCreatePersonByIdQuery request, CancellationToken cancellationToken)
  {
    if (!request.Username.IsValid || !request.PersonId)
    {
      return Result.Fail<Person>("Invalid username or person id");
    }

    var userResult = await personStore.GetAsync(request.PersonId, cancellationToken);
    return userResult.IsSuccess ? userResult : await personStore.CreateAsync(request.PersonId, request.Username, cancellationToken);
  }
}
