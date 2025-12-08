using Apollo.Core.People;
using Apollo.Database.People.Events;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

using Marten;

namespace Apollo.Database.People;

public sealed class PersonStore(SuperAdminConfig SuperAdminConfig, IDocumentSession session, TimeProvider timeProvider) : IPersonStore
{
  public async Task<Result<Person>> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbUserResult = await session.Query<DbPerson>().FirstOrDefaultAsync(u => u.Username == username.Value && u.Platform == username.Platform, cancellationToken);

      return dbUserResult is not null ? Result.Ok((Person)dbUserResult) : Result.Fail<Person>($"User with username {username.Value} not found");
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<Person>> CreateAsync(PersonId id, Username username, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcNow().DateTime;
      var pce = new PersonCreatedEvent(id, username.Value, username.Platform, time);

      var events = new List<object> { pce };

      if (IsSuperAdmin(username))
      {
        events.Add(new AccessGrantedEvent(id.Value, time));
      }

      _ = session.Events.StartStream<DbPerson>(id.Value, events);
      await session.SaveChangesAsync(cancellationToken);

      var newPerson = await session.Events.AggregateStreamAsync<DbPerson>(id.Value, token: cancellationToken);

      return newPerson is null ? Result.Fail<Person>($"Failed to create new user {username}") : Result.Ok((Person)newPerson);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  private bool IsSuperAdmin(Username username)
  {
    return !string.IsNullOrWhiteSpace(SuperAdminConfig.DiscordUsername)
      && username.Platform == Platform.Discord
      && string.Equals(username.Value, SuperAdminConfig.DiscordUsername, StringComparison.OrdinalIgnoreCase);
  }

  public async Task<Result<HasAccess>> GetAccessAsync(PersonId id, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbUser = await GetAsync(id, cancellationToken);

      return dbUser.IsFailed ? Result.Fail<HasAccess>(dbUser.Errors) : Result.Ok(dbUser.Value.HasAccess);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<Person>> GetAsync(PersonId id, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbUser = await session.Query<DbPerson>().FirstOrDefaultAsync(u => u.Id == id.Value, cancellationToken);
      return dbUser is null ? Result.Fail<Person>($"User with ID {id} not found") : Result.Ok((Person)dbUser);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> GrantAccessAsync(PersonId id, CancellationToken cancellationToken = default)
  {
    try
    {
      _ = session.Events.Append(id.Value, new AccessGrantedEvent(id.Value, timeProvider.GetUtcNow().DateTime));

      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
