using Apollo.Core.Data;
using Apollo.Core.People;
using Apollo.Database.People.Events;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

using JasperFx.Events.Projections;

using Marten;

using DbPerson = Apollo.Database.People.Person;
using MPerson = Apollo.Domain.People.Models.Person;

namespace Apollo.Database.People;

public sealed class PersonStore : IPersonStore, IDisposable
{
  private ApolloConnectionString ConnectionString { get; init; }
  private IDocumentSession Session { get; init; }

  private DocumentStore Store { get; init; }

  public PersonStore(ApolloConnectionString connectionString)
  {
    ConnectionString = connectionString;
    Store = DocumentStore.For(opts =>
    {
      opts.Connection(ConnectionString.Value);

      _ = opts.Schema.For<DbPerson>()
        .Identity(x => x.Id)
        .UniqueIndex(x => x.Username);

      _ = opts.Events.AddEventType<PersonCreatedEvent>();

      opts.Projections.Add<PersonProjection>(ProjectionLifecycle.Inline);
    });
    Session = Store.LightweightSession();
  }

  public void Dispose()
  {
    Session.Dispose();
  }

  public async Task<Result<MPerson>> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbUserResult = await Session.Query<DbPerson>().FirstOrDefaultAsync(u => u.Username == username.Value && u.Platform == username.Platform, cancellationToken);

      return dbUserResult is not null ? Result.Ok((MPerson)dbUserResult) : Result.Fail<MPerson>($"User with username {username.Value} not found");
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<MPerson>> CreateAsync(PersonId Id, Username username, CancellationToken cancellationToken = default)
  {
    try
    {
      var pce = new PersonCreatedEvent(Id, username.Value, username.Platform, DateTime.UtcNow);

      _ = Session.Events.StartStream<DbPerson>(Id.Value, pce);
      await Session.SaveChangesAsync(cancellationToken);

      var newPerson = await Session.Events.AggregateStreamAsync<DbPerson>(Id.Value, token: cancellationToken);

      return newPerson is null ? Result.Fail<MPerson>($"Failed to create new user {username}") : Result.Ok((MPerson)newPerson);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<HasAccess>> GetAccessAsync(PersonId Id, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbUser = await GetAsync(Id, cancellationToken);

      return dbUser.IsFailed ? Result.Fail<HasAccess>(dbUser.Errors) : Result.Ok(dbUser.Value.HasAccess);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<MPerson>> GetAsync(PersonId Id, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbUser = await Session.Query<DbPerson>().FirstOrDefaultAsync(u => u.Id == Id.Value, cancellationToken);
      return dbUser is null ? Result.Fail<MPerson>($"User with ID {Id} not found") : Result.Ok((MPerson)dbUser);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<MPerson>> GetUserByUsernameAsync(Username username, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbUser = await Session.Query<DbPerson>().FirstOrDefaultAsync(u => u.Username == username.Value, cancellationToken);

      return dbUser is null ? Result.Fail<MPerson>($"User with username {username.Value} not found") : Result.Ok((MPerson)dbUser);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> GrantAccessAsync(PersonId Id, CancellationToken cancellationToken = default)
  {
    try
    {
      _ = Session.Events.Append(Id.Value, new AccessGrantedEvent(Id.Value, DateTime.UtcNow));

      await Session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
