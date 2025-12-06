using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Core.People;

public interface IPersonStore
{
  Task<Result<Person>> CreateAsync(PersonId userId, Username username, CancellationToken cancellationToken = default);
  Task<Result<HasAccess>> GetAccessAsync(PersonId userId, CancellationToken cancellationToken = default);
  Task<Result<Person>> GetAsync(PersonId userId, CancellationToken cancellationToken = default);
  Task<Result<Person>> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default);
  Task<Result> GrantAccessAsync(PersonId userId, CancellationToken cancellationToken = default);
}
