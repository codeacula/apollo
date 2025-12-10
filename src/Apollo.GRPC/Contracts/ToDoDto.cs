using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record ToDoDto
{
  [DataMember]
  public required Guid Id { get; init; }

  [DataMember]
  public required Guid PersonId { get; init; }

  [DataMember]
  public required string Description { get; init; }

  [DataMember]
  public DateTime? ReminderDate { get; init; }

  [DataMember]
  public DateTime CreatedOn { get; init; }

  [DataMember]
  public DateTime UpdatedOn { get; init; }
}
