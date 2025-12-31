using Microsoft.Extensions.Logging;

namespace Apollo.Core.Logging;

/// <summary>
/// High-performance logging definitions for data access operations.
/// EventIds: 3200-3299
/// </summary>
public static partial class DataAccessLogs
{
  [LoggerMessage(
    EventId = 3200,
    Level = LogLevel.Warning,
    Message = "User not found or invalid username: {Username}")]
  public static partial void UserNotFound(ILogger logger, string username);

  [LoggerMessage(
    EventId = 3201,
    Level = LogLevel.Debug,
    Message = "Mock user access checked for {Username}: {HasAccess}")]
  public static partial void UserAccessChecked(ILogger logger, string username, bool hasAccess);

  [LoggerMessage(
    EventId = 3202,
    Level = LogLevel.Error,
    Message = "Unable to save message to conversation {ConversationId}: {Message}")]
  public static partial void UnableToSaveMessageToConversation(ILogger logger, Guid conversationId, string message);

  [LoggerMessage(
    EventId = 3203,
    Level = LogLevel.Warning,
    Message = "Failed to add notification channel for user {Username}: {ErrorMessage}")]
  public static partial void FailedToAddNotificationChannel(ILogger logger, string username, string errorMessage);

  [LoggerMessage(
    EventId = 3204,
    Level = LogLevel.Error,
    Message = "Unhandled exception processing message for user {Username}")]
  public static partial void UnhandledMessageProcessingError(ILogger logger, Exception exception, string username);
}
