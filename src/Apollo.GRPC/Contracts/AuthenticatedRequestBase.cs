using System.Runtime.Serialization;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.GRPC.Contracts;

[DataContract]
public abstract record AuthenticatedRequestBase : IAuthenticatedRequest
{
  [DataMember(Order = 101)]
  public required Platform Platform { get; init; }

  [DataMember(Order = 102)]
  public required string PlatformUserId { get; init; }

  [DataMember(Order = 103)]
  public required string Username { get; init; }

  public PlatformId ToPlatformId() => new(Username, PlatformUserId, Platform);
}
