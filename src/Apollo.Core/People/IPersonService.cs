using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Core.People;

public interface IPersonService
{
  Task<Result<Person>> GetOrCreateAsync(PlatformId platformId, CancellationToken cancellationToken = default);
  Task<Result<HasAccess>> HasAccessAsync(PersonId personId, CancellationToken cancellationToken = default);
  Task<Result> MapPlatformIdToPersonIdAsync(PlatformId platformId, PersonId personId, CancellationToken cancellationToken = default);
}
