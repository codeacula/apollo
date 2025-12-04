
using Apollo.Core.Infrastructure.Database.Stores;
using Apollo.Domain.Users.Models;
using Apollo.Domain.Users.ValueObjects;

using FluentResults;

namespace Apollo.Database.Stores;

public sealed class ApolloUserStore : IApolloUserStore
{
  public Task<Result<User>> GetOrCreateUserAsync(Username username, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<Result<HasAccess>> GetUserAccessAsync(Username username, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
