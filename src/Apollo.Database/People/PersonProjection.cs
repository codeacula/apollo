using Apollo.Database.People.Events;

using JasperFx.Events;

using Marten.Events.Aggregation;

namespace Apollo.Database.People;

public class PersonProjection : SingleStreamProjection<Person, Guid>
{
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
