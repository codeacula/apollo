
using Apollo.Core.Infrastructure.Data;
using Apollo.Domain.Users.Models;
using Apollo.Domain.Users.ValueObjects;

using FluentResults;

using Marten;

namespace Apollo.Database.Stores;

public sealed class ApolloUserStore(ApolloConnectionString connectionString) : IApolloUserStore
{
  private readonly IDocumentStore _store = DocumentStore.For(_ => _.Connection(connectionString.Value));

  public Task<Result<User>> GetOrCreateUserAsync(Username username, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<Result<HasAccess>> GetUserAccessAsync(Username username, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
