using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record ReminderDTO
{
  [DataMember(Order = 1)]
  public required Guid Id { get; init; }

  [DataMember(Order = 2)]
  public required Guid PersonId { get; init; }

  [DataMember(Order = 3)]
  public required string Details { get; init; }

  [DataMember(Order = 4)]
  public required DateTime ReminderTime { get; init; }

  [DataMember(Order = 5)]
  public DateTime CreatedOn { get; init; }

  [DataMember(Order = 6)]
  public DateTime UpdatedOn { get; init; }
}
