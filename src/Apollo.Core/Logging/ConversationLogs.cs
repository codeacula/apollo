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
    Message = "Two-phase AI request started for person {PersonId}: {UserMessage}")]
  public static partial void TwoPhaseRequestStarted(ILogger logger, Guid personId, string userMessage);

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
}
