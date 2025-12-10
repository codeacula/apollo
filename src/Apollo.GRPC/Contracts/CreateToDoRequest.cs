using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record CreateToDoRequest
{
  [DataMember(Order = 1)]
  public required Guid PersonId { get; init; }

  [DataMember(Order = 2)]
  public required string Description { get; init; }

  [DataMember(Order = 3)]
  public DateTime? ReminderDate { get; init; }
}
