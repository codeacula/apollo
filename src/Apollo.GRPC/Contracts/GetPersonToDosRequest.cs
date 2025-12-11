using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record GetPersonToDosRequest
{
  [DataMember(Order = 1)]
  public required Guid PersonId { get; init; }
}
