using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record DailyPlanDTO
{
  [DataMember(Order = 1)]
  public required DailyPlanTaskDTO[] SuggestedTasks { get; init; }

  [DataMember(Order = 2)]
  public required string SelectionRationale { get; init; }

  [DataMember(Order = 3)]
  public required int TotalActiveTodos { get; init; }
}

[DataContract]
public sealed record DailyPlanTaskDTO
{
  [DataMember(Order = 1)]
  public required Guid Id { get; init; }

  [DataMember(Order = 2)]
  public required string Description { get; init; }

  [DataMember(Order = 3)]
  public required int Priority { get; init; }

  [DataMember(Order = 4)]
  public required int Energy { get; init; }

  [DataMember(Order = 5)]
  public required int Interest { get; init; }

  [DataMember(Order = 6)]
  public DateTime? DueDate { get; init; }
}
