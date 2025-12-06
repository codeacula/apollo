using Apollo.Database.People.Events;
using Apollo.Domain.Common.Enums;

using JasperFx.Events;

using MPerson = Apollo.Domain.People.Models.Person;

namespace Apollo.Database.People;

public sealed record Person
{
  public Guid Id { get; init; }
  public required string Username { get; init; }
  public Platform Platform { get; init; }
  public bool HasAccess { get; init; }
  public DateTime CreatedOn { get; init; }
  public DateTime UpdatedOn { get; init; }

  public static explicit operator MPerson(Person person)
  {
    return new()
    {
      Id = new(person.Id),
      Username = new(person.Username, person.Platform),
      HasAccess = new(person.HasAccess),
      CreatedOn = new(person.CreatedOn),
      UpdatedOn = new(person.UpdatedOn)
    };
  }

  public static Person Create(IEvent<PersonCreatedEvent> ev)
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

  public static Person Apply(IEvent<AccessGrantedEvent> ev, Person person)
  {
    return person with
    {
      HasAccess = true,
      UpdatedOn = ev.Data.GrantedOn
    };
  }

  public static Person Apply(IEvent<AccessRevokedEvent> ev, Person person)
  {
    return person with
    {
      HasAccess = false,
      UpdatedOn = ev.Data.RevokedOn
    };
  }

  public static Person Apply(IEvent<PersonUpdatedEvent> ev, Person person)
  {
    return person with
    {
      UpdatedOn = ev.Data.UpdatedOn
    };
  }
}
