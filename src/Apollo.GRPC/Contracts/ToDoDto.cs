using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record ToDoDTO
{
  [DataMember(Order = 1)]
  public required Guid Id { get; init; }

  [DataMember(Order = 3)]
  public required string Description { get; init; }

  [DataMember(Order = 4)]
  public DateTime? ReminderDate { get; init; }

  [DataMember(Order = 5)]
  public DateTime CreatedOn { get; init; }

  [DataMember(Order = 6)]
  public DateTime UpdatedOn { get; init; }

  [DataMember(Order = 7)]
  public required Platform PersonPlatform { get; init; }

  [DataMember(Order = 8)]
  public required string PersonProviderId { get; init; }
}
