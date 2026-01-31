using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record ToDoDTO
{
  [DataMember(Order = 1)]
  public required Guid Id { get; init; }

  [DataMember(Order = 2)]
  public required Guid PersonId { get; init; }

  [DataMember(Order = 3)]
  public required string Description { get; init; }

  [DataMember(Order = 4)]
  public DateTime? ReminderDate { get; init; }

  [DataMember(Order = 5)]
  public DateTime CreatedOn { get; init; }

  [DataMember(Order = 6)]
  public DateTime UpdatedOn { get; init; }

  [DataMember(Order = 7)]
  public Level Priority { get; init; }

  [DataMember(Order = 8)]
  public Level Energy { get; init; }

  [DataMember(Order = 9)]
  public Level Interest { get; init; }
}
