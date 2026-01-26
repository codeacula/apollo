using System.Runtime.Serialization;

namespace Apollo.Core.ToDos.Responses;

[DataContract]
public sealed record ToDoSummary
{
  [DataMember(Order = 1)]
  public required Guid Id { get; init; }

  [DataMember(Order = 2)]
  public required string Description { get; init; }

  [DataMember(Order = 3)]
  public DateTime? ReminderDate { get; init; }

  [DataMember(Order = 4)]
  public DateTime CreatedOn { get; init; }

  [DataMember(Order = 5)]
  public DateTime UpdatedOn { get; init; }
}
