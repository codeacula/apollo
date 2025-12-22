using Apollo.Database.People.Events;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using JasperFx.Events;

namespace Apollo.Database.People;

public sealed record DbPerson
{
  public Guid Id { get; init; }
  public required string Username { get; init; }
  public Platform Platform { get; init; }
  public bool HasAccess { get; init; }
  public string? TimeZoneId { get; init; }
  public DateTime CreatedOn { get; init; }
  public DateTime UpdatedOn { get; init; }

  public static explicit operator Person(DbPerson person)
  {
    PersonTimeZoneId? timeZoneId = null;
    if (person.TimeZoneId is not null && PersonTimeZoneId.TryParse(person.TimeZoneId, out var parsedTimeZone, out _))
    {
      timeZoneId = parsedTimeZone;
    }

    return new()
    {
      Id = new(person.Id),
      Username = new(person.Username, person.Platform),
      HasAccess = new(person.HasAccess),
      TimeZoneId = timeZoneId,
      CreatedOn = new(person.CreatedOn),
      UpdatedOn = new(person.UpdatedOn)
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
}
