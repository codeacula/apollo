using JasperFx.Events;

using Marten.Events.Aggregation;

namespace Apollo.Database.Users;

public class ApolloUserProjection : SingleStreamProjection<ApolloUser, Guid>
{
  public static ApolloUser Create(IEvent<UserCreatedEvent> ev)
  {
    var eventData = ev.Data;

    return new()
    {
      Id = eventData.UserId,
      Username = eventData.Username,
      HasAccess = false,
      CreatedAt = eventData.CreatedAt,
      UpdatedAt = eventData.CreatedAt
    };
  }
}
