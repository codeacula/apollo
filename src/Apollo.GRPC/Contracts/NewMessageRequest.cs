using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record NewMessageRequest : AuthenticatedRequestBase
{
  [DataMember(Order = 1)]
  public required string Content { get; init; }
}
