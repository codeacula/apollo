using Apollo.Core.Data;
using Apollo.Core.People;
using Apollo.Database.People.Events;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

using Marten;

using DbPerson = Apollo.Database.People.Person;
using MPerson = Apollo.Domain.People.Models.Person;

namespace Apollo.Database.People;

public sealed class PersonStore(SuperAdminConfig SuperAdminConfig, IDocumentSession session, TimeProvider timeProvider) : IPersonStore
{
  public async Task<Result<MPerson>> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbUserResult = await session.Query<DbPerson>().FirstOrDefaultAsync(u => u.Username == username.Value && u.Platform == username.Platform, cancellationToken);

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
      var time = timeProvider.GetUtcNow().DateTime;
      var pce = new PersonCreatedEvent(Id, username.Value, username.Platform, time);

      var events = new List<object> { pce };

      if (IsSuperAdmin(username))
      {
        events.Add(new AccessGrantedEvent(Id.Value, time));
      }

      _ = session.Events.StartStream<DbPerson>(Id.Value, events);
      await session.SaveChangesAsync(cancellationToken);

      var newPerson = await session.Events.AggregateStreamAsync<DbPerson>(Id.Value, token: cancellationToken);

      return newPerson is null ? Result.Fail<MPerson>($"Failed to create new user {username}") : Result.Ok((MPerson)newPerson);
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
      var dbUser = await session.Query<DbPerson>().FirstOrDefaultAsync(u => u.Id == Id.Value, cancellationToken);
      return dbUser is null ? Result.Fail<MPerson>($"User with ID {Id} not found") : Result.Ok((MPerson)dbUser);
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
      _ = session.Events.Append(Id.Value, new AccessGrantedEvent(Id.Value, timeProvider.GetUtcNow().DateTime));

      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
