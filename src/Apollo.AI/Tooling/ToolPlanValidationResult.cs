using Apollo.AI.Models;
using Apollo.AI.Requests;

namespace Apollo.AI.Tooling;

public sealed record ToolPlanValidationResult
{
  public List<PlannedToolCall> ApprovedCalls { get; init; } = [];
  public List<ToolCallResult> BlockedCalls { get; init; } = [];
}
