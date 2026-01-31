using Microsoft.Extensions.Logging;

namespace Apollo.Core.Logging;

/// <summary>
/// High-performance logging definitions for conversation operations.
/// EventIds: 3300-3399
/// </summary>
public static partial class ConversationLogs
{
  [LoggerMessage(
    EventId = 3300,
    Level = LogLevel.Debug,
    Message = "Three-step AI request started for person {PersonId}: {UserMessage}")]
  public static partial void ThreeStepRequestStarted(ILogger logger, Guid personId, string userMessage);

  [LoggerMessage(
    EventId = 3301,
    Level = LogLevel.Information,
    Message = "Actions taken for person {PersonId}: {Actions}")]
  public static partial void ActionsTaken(ILogger logger, Guid personId, List<string> actions);

  [LoggerMessage(
    EventId = 3302,
    Level = LogLevel.Warning,
    Message = "Failed to get ToDos context for person {PersonId}: {ErrorMessage}")]
  public static partial void FailedToGetToDosContext(ILogger logger, Guid personId, string errorMessage);

  [LoggerMessage(
    EventId = 3303,
    Level = LogLevel.Warning,
    Message = "Tool plan parsing failed for person {PersonId}: {ErrorMessage}")]
  public static partial void ToolPlanParsingFailed(ILogger logger, Guid personId, string? errorMessage);

  [LoggerMessage(
    EventId = 3304,
    Level = LogLevel.Warning,
    Message = "Tool planning request failed for person {PersonId}: {ErrorMessage}")]
  public static partial void ToolPlanningRequestFailed(ILogger logger, Guid personId, string? errorMessage);

  [LoggerMessage(
    EventId = 3305,
    Level = LogLevel.Information,
    Message = "Tool execution completed for person {PersonId}: {ApprovedCount} approved, {BlockedCount} blocked, {ExecutedCount} executed")]
  public static partial void ToolExecutionCompleted(ILogger logger, Guid personId, int approvedCount, int blockedCount, int executedCount);

  [LoggerMessage(
    EventId = 3306,
    Level = LogLevel.Debug,
    Message = "Tool plan received for person {PersonId}: {ToolCallCount} tool calls")]
  public static partial void ToolPlanReceived(ILogger logger, Guid personId, int toolCallCount);
}
