
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

    var userId = Guid.NewGuid();
    var userCreated = new UserCreatedEvent(userId, username, DateTime.UtcNow);

    _ = session.Events.StartStream<ApolloUser>(userId, userCreated);
    await session.SaveChangesAsync(cancellationToken);

    var newUser = await session.Events.AggregateStreamAsync<ApolloUser>(userId, token: cancellationToken);

    return newUser is null ? Result.Fail<User>($"Failed to create new user {username}") : Result.Ok<User>(newUser);
  }

  public Task<Result<HasAccess>> GetUserAccessAsync(Username username, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
