using Apollo.Domain.People.Models;

using JasperFx.Events;

using Marten.Events.Aggregation;

namespace Apollo.Database.People;

public class ApolloUserProjection : SingleStreamProjection<Person, Guid>
{
  public static Person Create(IEvent<UserCreatedEvent> ev)
  {
    var eventData = ev.Data;

    return new()
    {
      Id = eventData.UserId,
      Username = eventData.Username,
      HasAccess = new(false),
      CreatedOn = eventData.CreatedOn,
      UpdatedOn = eventData.CreatedOn
    };
  }
}
