
using Apollo.Core.Infrastructure.Data;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

using JasperFx.Events.Projections;

using Marten;

namespace Apollo.Database.People;

public sealed class ApolloUserStore : IApolloUserStore, IDisposable
{
  private ApolloConnectionString ConnectionString { get; init; }
  private IDocumentSession Session { get; init; }

  private DocumentStore Store { get; init; }

  public ApolloUserStore(ApolloConnectionString connectionString)
  {
    ConnectionString = connectionString;
    Store = DocumentStore.For(opts =>
    {
      opts.Connection(ConnectionString.Value);

      _ = opts.Schema.For<ApolloUser>()
        .Identity(x => x.Id)
        .UniqueIndex(x => x.Username);

      _ = opts.Events.AddEventType<UserCreatedEvent>();

      opts.Projections.Add<ApolloUserProjection>(ProjectionLifecycle.Inline);
    });
    Session = Store.LightweightSession();
  }

  public void Dispose()
  {
    Session.Dispose();
  }

  public async Task<Result<Person>> GetOrCreateUserByUsernameAsync(Username username, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbUserResult = await GetUserByUsernameAsync(username, cancellationToken);

      if (dbUserResult.IsSuccess)
      {
        return Result.Ok(dbUserResult.Value);
      }

      var userId = Guid.NewGuid();
      var userCreated = new UserCreatedEvent(userId, username.Value, DateTime.UtcNow);

      _ = Session.Events.StartStream<ApolloUser>(userId, userCreated);
      await Session.SaveChangesAsync(cancellationToken);

      var newUser = await Session.Events.AggregateStreamAsync<ApolloUser>(userId, token: cancellationToken);

      return newUser is null ? Result.Fail<Person>($"Failed to create new user {username}") : Result.Ok<Person>(newUser);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<HasAccess>> GetUserAccessAsync(PersonId userId, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbUser = await GetUserAsync(userId, cancellationToken);

      return dbUser.IsFailed ? Result.Fail<HasAccess>(dbUser.Errors) : Result.Ok(dbUser.Value.HasAccess);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<Person>> GetUserAsync(PersonId userId, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbUser = await Session.Query<ApolloUser>().FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

      return dbUser is null ? Result.Fail<Person>($"User with ID {userId} not found") : Result.Ok<Person>(dbUser);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<Person>> GetUserByUsernameAsync(Username username, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbUser = await Session.Query<ApolloUser>().FirstOrDefaultAsync(u => u.Username == username.Value, cancellationToken);

      return dbUser is null ? Result.Fail<Person>($"User with username {username.Value} not found") : Result.Ok<Person>(dbUser);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> GrantAccessAsync(PersonId userId, CancellationToken cancellationToken = default)
  {
    try
    {
      _ = Session.Events.Append(userId.Value, new UserGrantedAccessEvent(userId.Value, DateTime.UtcNow));

      await Session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
