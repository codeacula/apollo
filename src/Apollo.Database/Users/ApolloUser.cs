using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Users.Models;
using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Database.Users;

public sealed record ApolloUser
{
  public Guid UserId { get; init; }

  public string Username { get; init; } = "";

  public bool HasAccess { get; init; }

  public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

  public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

  public static ApolloUser Create(UserCreatedEvent userCreatedEvent)
  {
    return new()
    {
      UserId = userCreatedEvent.UserId,
      Username = userCreatedEvent.Username,
      HasAccess = false,
      CreatedAt = userCreatedEvent.CreatedAt,
      UpdatedAt = userCreatedEvent.CreatedAt
    };
  }

  public static ApolloUser Apply(UserGrantedAccessEvent grantedAccessEvent, ApolloUser user)
  {
    return user with
    {
      HasAccess = true,
      UpdatedAt = grantedAccessEvent.GrantedAt
    };
  }

  public static implicit operator User(ApolloUser value)
  {
    return new()
    {
      CreatedOn = new CreatedOn(value.CreatedAt),
      HasAccess = new HasAccess(value.HasAccess),
      Id = new UserId(value.UserId),
      Username = new Username(value.Username),
      UpdatedOn = new UpdatedOn(value.UpdatedAt)
    };
  }
}
