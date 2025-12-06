using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Core.People;

public interface IPersonService
{
  Task<Result<Person>> GetOrCreateAsync(Username username, CancellationToken cancellationToken = default);
  Task<Result> GrantAccessAsync(Username username, CancellationToken cancellationToken = default);
  Task<Result<HasAccess>> UserHasAccessAsync(Username username, CancellationToken cancellationToken = default);
}
