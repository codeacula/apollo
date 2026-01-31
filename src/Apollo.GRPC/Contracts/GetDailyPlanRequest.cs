using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record GetDailyPlanRequest
{
  [DataMember(Order = 1)]
  public required string PlatformUserId { get; init; }

  [DataMember(Order = 2)]
  public required Platform Platform { get; init; }

  [DataMember(Order = 3)]
  public required string Username { get; init; }
}
