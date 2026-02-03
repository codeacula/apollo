using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record GetToDoRequest : AuthenticatedRequestBase
{
  [DataMember(Order = 1)]
  public required Guid ToDoId { get; init; }
}
