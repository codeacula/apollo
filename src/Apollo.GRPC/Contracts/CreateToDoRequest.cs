using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record CreateToDoRequest
{
  [DataMember]
  public required Guid PersonId { get; init; }

  [DataMember]
  public required string Description { get; init; }

  [DataMember]
  public DateTime? ReminderDate { get; init; }
}
