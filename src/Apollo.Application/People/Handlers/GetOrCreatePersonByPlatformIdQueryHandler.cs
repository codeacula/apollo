using Apollo.Application.People.Queries;
using Apollo.Core.People;
using Apollo.Domain.People.Models;

using FluentResults;

namespace Apollo.Application.People.Handlers;

public sealed class GetOrCreatePersonByPlatformIdQueryHandler(IPersonStore personStore)
  : IRequestHandler<GetOrCreatePersonByPlatformIdQuery, Result<Person>>
{
  public async Task<Result<Person>> Handle(GetOrCreatePersonByPlatformIdQuery request, CancellationToken cancellationToken)
  {
    var userResult = await personStore.GetByPlatformIdAsync(request.PlatformId, cancellationToken);
    return userResult.IsSuccess ? userResult : await personStore.CreateByPlatformIdAsync(request.PlatformId, cancellationToken);
  }
}
