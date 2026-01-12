using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Core.People;

public interface IPersonService
{
  Task<Result<Person>> GetOrCreateAsync(PlatformId platformId, CancellationToken ct = default);
  Task<Result<HasAccess>> HasAccessAsync(PlatformId platformId);
  Task<Result> MapPlatformIdToPersonIdAsync(PlatformId platformId, PersonId personId, CancellationToken ct = default);
}
