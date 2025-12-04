using Apollo.Domain.Users.Models;
using Apollo.Domain.Users.ValueObjects;

using FluentResults;

namespace Apollo.Core.Infrastructure.Data;

public interface IApolloUserStore
{
  Task<Result<User>> GetOrCreateUserAsync(Username username, CancellationToken cancellationToken = default);
  Task<Result<HasAccess>> GetUserAccessAsync(Username username, CancellationToken cancellationToken = default);
}
