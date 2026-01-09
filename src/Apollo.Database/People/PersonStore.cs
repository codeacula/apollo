using Apollo.Core;
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
  public async Task<Result<Person>> CreateByPlatformIdAsync(PlatformId platformId, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcDateTime();
      var id = Guid.NewGuid();
      var pce = new PersonCreatedEvent(id, platformId.Username, platformId.Platform, platformId.PlatformUserId, time);

      var events = new List<object> { pce };

      if (IsSuperAdmin(platformId))
      {
        events.Add(new AccessGrantedEvent(id, time));
      }

      _ = session.Events.StartStream<DbPerson>(id, events);
      await session.SaveChangesAsync(cancellationToken);

      var newPerson = await session.Events.AggregateStreamAsync<DbPerson>(id, token: cancellationToken);

      return newPerson is null ? Result.Fail<Person>($"Failed to create new user {platformId.Username}") : Result.Ok((Person)newPerson);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  private bool IsSuperAdmin(PlatformId platformId)
  {
    return !string.IsNullOrWhiteSpace(SuperAdminConfig.DiscordUserId)
      && platformId.Platform == Platform.Discord
      && string.Equals(platformId.PlatformUserId, SuperAdminConfig.DiscordUserId, StringComparison.OrdinalIgnoreCase);
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
      var dbUser = await session.Query<DbPerson>()
        .FirstOrDefaultAsync(u => u.Id == id.Value, cancellationToken);
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
      _ = session.Events.Append(id.Value, new AccessGrantedEvent(id.Value, timeProvider.GetUtcDateTime()));

      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> SetTimeZoneAsync(PersonId id, PersonTimeZoneId timeZoneId, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcDateTime();
      _ = session.Events.Append(id.Value, new PersonTimeZoneUpdatedEvent(id.Value, timeZoneId.Value, time));

      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> AddNotificationChannelAsync(Person person, NotificationChannel channel, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcDateTime();
      _ = session.Events.Append(
        person.Id.Value,
        new NotificationChannelAddedEvent(person.PlatformId.Platform, person.PlatformId.PlatformUserId, channel.Type, channel.Identifier, time));

      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> RemoveNotificationChannelAsync(Person person, NotificationChannel channel, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcDateTime();
      _ = session.Events.Append(
        person.Id.Value,
        new NotificationChannelRemovedEvent(person.PlatformId.Platform, person.PlatformId.PlatformUserId, channel.Type, channel.Identifier, time));

      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> ToggleNotificationChannelAsync(Person person, NotificationChannel channel, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcDateTime();
      _ = session.Events.Append(
        person.Id.Value,
        new NotificationChannelToggledEvent(
          person.PlatformId.Platform,
          person.PlatformId.PlatformUserId,
          channel.Type,
          channel.Identifier,
          channel.IsEnabled,
          time));

      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> EnsureNotificationChannelAsync(Person person, NotificationChannel channel, CancellationToken cancellationToken = default)
  {
    try
    {
      // Check if person already has a channel of this type
      NotificationChannel? existingChannel = person.NotificationChannels.FirstOrDefault(c => c.Type == channel.Type);
      if (existingChannel is not null)
      {

        var ch = (NotificationChannel)existingChannel;
        // If the identifier is the same, nothing to do
        if (ch.Identifier == channel.Identifier)
        {
          return Result.Ok();
        }

        var removeResult = await RemoveNotificationChannelAsync(person, ch, cancellationToken);
        return removeResult.IsFailed ? removeResult : await AddNotificationChannelAsync(person, channel, cancellationToken);
      }

      // No channel of this type exists yet: add the channel
      return await AddNotificationChannelAsync(person, channel, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
