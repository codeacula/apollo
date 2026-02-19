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
