using Apollo.Application.People.Queries;
using Apollo.Core.People;
using Apollo.Domain.People.Models;

using FluentResults;

namespace Apollo.Application.People.Handlers;

public sealed class GetOrCreatePersonByUsernameQueryHandler(IPersonStore personStore)
  : IRequestHandler<GetOrCreatePersonByUsernameQuery, Result<Person>>
{
  public async Task<Result<Person>> Handle(GetOrCreatePersonByUsernameQuery request, CancellationToken cancellationToken)
  {
    if (!request.Username.IsValid)
    {
      return Result.Fail<Person>("Invalid username");
    }

    var userResult = await personStore.GetByUsernameAsync(request.Username, cancellationToken);
    return userResult.IsSuccess ? userResult : await personStore.CreateAsync(new(Guid.NewGuid()), request.Username, cancellationToken);
  }
}
