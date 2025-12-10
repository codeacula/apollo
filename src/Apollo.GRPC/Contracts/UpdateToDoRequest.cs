using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record UpdateToDoRequest
{
  [DataMember]
  public required Guid ToDoId { get; init; }

  [DataMember]
  public required string Description { get; init; }
}
