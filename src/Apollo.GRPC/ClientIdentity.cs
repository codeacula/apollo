namespace Apollo.GRPC;

[ProtoContract]
public sealed record ClientIdentity
{
  [ProtoMember(1)]
  public required Platform Platform { get; init; }

  [ProtoMember(2)]
  public required string ChannelId { get; init; }

  [ProtoMember(3)]
  public required string UserId { get; init; }
}
