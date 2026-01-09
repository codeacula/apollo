using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Core.People;

public interface IPersonService
{
  Task<Result<Person>> GetOrCreateAsync(PersonId personId, Username username, CancellationToken cancellationToken = default);
  Task<Result> GrantAccessAsync(PersonId personId, CancellationToken cancellationToken = default);
  Task<Result<HasAccess>> HasAccessAsync(PersonId personId, CancellationToken cancellationToken = default);
}
