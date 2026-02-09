using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record GetPersonToDosRequest : AuthenticatedRequestBase
{
  [DataMember(Order = 1)]
  public bool IncludeCompleted { get; init; }
}
