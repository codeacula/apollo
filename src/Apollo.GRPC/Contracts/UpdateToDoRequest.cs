using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record UpdateToDoRequest
{
  [DataMember(Order = 1)]
  public required Guid ToDoId { get; init; }

  [DataMember(Order = 2)]
  public required string Description { get; init; }
}
