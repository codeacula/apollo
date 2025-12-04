using Apollo.Domain.Users.Models;
using Apollo.Domain.Users.ValueObjects;

using FluentResults;

namespace Apollo.Core.Infrastructure.Services;

public interface IApolloUserService
{
  Task<Result<User>> GetOrCreateUserAsync(Username username, CancellationToken cancellationToken = default);
  Task<Result<bool>> UserHasAccessAsync(Username username, CancellationToken cancellationToken = default);
}
