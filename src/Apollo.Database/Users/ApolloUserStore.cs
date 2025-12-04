
using Apollo.Core.Infrastructure.Data;
using Apollo.Domain.Users.Models;
using Apollo.Domain.Users.ValueObjects;

using FluentResults;

using Marten;

namespace Apollo.Database.Users;

public sealed class ApolloUserStore(ApolloConnectionString connectionString) : IApolloUserStore
{
  private readonly DocumentStore _store = DocumentStore.For(opts =>
  {
    opts.Connection(connectionString.Value);

    _ = opts.Events.AddEventType<UserCreatedEvent>();

    _ = opts.Projections.LiveStreamAggregation<ApolloUser>();
  });

  public async Task<Result<User>> GetOrCreateUserAsync(Username username, CancellationToken cancellationToken = default)
  {
    await using var session = _store.LightweightSession();

    var dbUser = session.Query<ApolloUser>()
      .Where(u => u.Username == username)
      .FirstOrDefault();

    if (dbUser is not null)
    {
      return Result.Ok<User>(dbUser);
    }

    throw new NotImplementedException();
  }

  public Task<Result<HasAccess>> GetUserAccessAsync(Username username, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
