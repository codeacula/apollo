using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record ClientIdentity
{
  [DataMember(Order = 1)]
  public required Platform Platform { get; init; }

  [DataMember(Order = 2)]
  public required string ChannelId { get; init; }

  [DataMember(Order = 3)]
  public required string UserId { get; init; }
}
