using Apollo.Database.People.Events;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using JasperFx.Events;

namespace Apollo.Database.People;

public sealed record DbPerson
{
  public required Guid Id { get; init; }
  public Platform Platform { get; init; }
  public required string PlatformUserId { get; init; }
  public required string Username { get; init; }
  public bool HasAccess { get; init; }
  public string? TimeZoneId { get; init; }
  public int? DailyTaskCount { get; init; }
  public ICollection<DbNotificationChannel> NotificationChannels { get; init; } = [];
  public DateTime CreatedOn { get; init; }
  public DateTime UpdatedOn { get; init; }

  public static explicit operator Person(DbPerson dbPerson)
  {
    PersonTimeZoneId? timeZoneId = null;
    if (dbPerson.TimeZoneId is not null && PersonTimeZoneId.TryParse(dbPerson.TimeZoneId, out var parsedTimeZone, out _))
    {
      timeZoneId = parsedTimeZone;
    }

    DailyTaskCount? dailyTaskCount = null;
    if (dbPerson.DailyTaskCount.HasValue && Domain.People.ValueObjects.DailyTaskCount.TryParse(dbPerson.DailyTaskCount.Value, out var parsedCount, out _))
    {
      dailyTaskCount = parsedCount;
    }

    var notificationChannels = dbPerson.NotificationChannels
      .Select(c => new NotificationChannel(c.Type, c.Identifier, c.IsEnabled))
      .ToList();

    return new()
    {
      Id = new(dbPerson.Id),
      PlatformId = new(dbPerson.Username, dbPerson.PlatformUserId, dbPerson.Platform),
      Username = new(dbPerson.Username),
      HasAccess = new(dbPerson.HasAccess),
      TimeZoneId = timeZoneId,
      DailyTaskCount = dailyTaskCount,
      NotificationChannels = notificationChannels,
      CreatedOn = new(dbPerson.CreatedOn),
      UpdatedOn = new(dbPerson.UpdatedOn)
    };
  }

  public static DbPerson Create(IEvent<PersonCreatedEvent> ev)
  {
    var eventData = ev.Data;

    return new()
    {
      Id = eventData.Id,
      Username = eventData.Username,
      HasAccess = false,
      Platform = eventData.Platform,
      PlatformUserId = eventData.PlatformUserId,
      CreatedOn = eventData.CreatedOn,
      UpdatedOn = eventData.CreatedOn
    };
  }

  public static DbPerson Apply(IEvent<AccessGrantedEvent> ev, DbPerson person)
  {
    return person with
    {
      HasAccess = true,
      UpdatedOn = ev.Data.GrantedOn
    };
  }

  public static DbPerson Apply(IEvent<AccessRevokedEvent> ev, DbPerson person)
  {
    return person with
    {
      HasAccess = false,
      UpdatedOn = ev.Data.RevokedOn
    };
  }

  public static DbPerson Apply(IEvent<PersonUpdatedEvent> ev, DbPerson person)
  {
    return person with
    {
      UpdatedOn = ev.Data.UpdatedOn
    };
  }

  public static DbPerson Apply(IEvent<PersonTimeZoneUpdatedEvent> ev, DbPerson person)
  {
    return person with
    {
      TimeZoneId = ev.Data.TimeZoneId,
      UpdatedOn = ev.Data.UpdatedOn
    };
  }

  public static DbPerson Apply(IEvent<PersonDailyTaskCountUpdatedEvent> ev, DbPerson person)
  {
    return person with
    {
      DailyTaskCount = ev.Data.DailyTaskCount,
      UpdatedOn = ev.Data.UpdatedOn
    };
  }

  public static DbPerson Apply(IEvent<NotificationChannelAddedEvent> ev, DbPerson person)
  {
    var newChannel = new DbNotificationChannel
    {
      PersonPlatform = ev.Data.PersonPlatform,
      PersonProviderId = ev.Data.PersonProviderId,
      Type = ev.Data.ChannelType,
      Identifier = ev.Data.Identifier,
      IsEnabled = true,
      CreatedOn = ev.Data.AddedOn,
      UpdatedOn = ev.Data.AddedOn
    };

    var channels = person.NotificationChannels.ToList();
    channels.Add(newChannel);

    return person with
    {
      NotificationChannels = channels,
      UpdatedOn = ev.Data.AddedOn
    };
  }

  public static DbPerson Apply(IEvent<NotificationChannelRemovedEvent> ev, DbPerson person)
  {
    var channels = person.NotificationChannels
      .Where(c => !(c.Type == ev.Data.ChannelType && c.Identifier == ev.Data.Identifier))
      .ToList();

    return person with
    {
      NotificationChannels = channels,
      UpdatedOn = ev.Data.RemovedOn
    };
  }

  public static DbPerson Apply(IEvent<NotificationChannelToggledEvent> ev, DbPerson person)
  {
    var channels = person.NotificationChannels.Select(c =>
    {
      return (c.Type, c.Identifier) == (ev.Data.ChannelType, ev.Data.Identifier)
        ? c with { IsEnabled = ev.Data.IsEnabled, UpdatedOn = ev.Data.ToggledOn }
        : c;
    }).ToList();

    return person with
    {
      NotificationChannels = channels,
      UpdatedOn = ev.Data.ToggledOn
    };
  }
}
