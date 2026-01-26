using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record NewMessageRequest
{
  [DataMember(Order = 1)]
  public required Platform Platform { get; init; }

  [DataMember(Order = 2)]
  public required string PlatformUserId { get; init; }

  [DataMember(Order = 3)]
  public required string Username { get; init; }

  [DataMember(Order = 4)]
  public required string Content { get; init; }

  public PlatformId ToPlatformId() => new(Username, PlatformUserId, Platform);
}
