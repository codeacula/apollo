using Microsoft.Extensions.Logging;

namespace Apollo.Core.Logging;

public static partial class ToDoLogs
{
  [LoggerMessage(
      EventId = 5000,
      Level = LogLevel.Information,
      Message = "To-Do Reminder Job started at {Time}.")]
  public static partial void LogJobStarted(ILogger logger, DateTimeOffset time);

  [LoggerMessage(
      EventId = 5001,
      Level = LogLevel.Error,
      Message = "Failed to retrieve To-Do items: {Message}")]
  public static partial void LogFailedToRetrieveToDos(ILogger logger, string message);

  [LoggerMessage(
      EventId = 5002,
      Level = LogLevel.Information,
      Message = "Found {Count} due To-Do items.")]
  public static partial void LogFoundDueToDos(ILogger logger, int count);

  [LoggerMessage(
      EventId = 5003,
      Level = LogLevel.Error,
      Message = "Failed to get person {PersonId} for To-Do {ToDoId}.")]
  public static partial void LogFailedToGetPerson(ILogger logger, Guid personId, Guid toDoId);

  [LoggerMessage(
      EventId = 5004,
      Level = LogLevel.Information,
      Message = "Sending reminder for To-Do {ToDoId} to user {Username}.")]
  public static partial void LogSendingReminder(ILogger logger, Guid toDoId, string username);

  [LoggerMessage(
      EventId = 5005,
      Level = LogLevel.Information,
      Message = "Reminder sent to user {Username}: {Description}")]
  public static partial void LogReminder(ILogger logger, string username, string description);

  [LoggerMessage(
      EventId = 5009,
      Level = LogLevel.Information,
      Message = "Sending reminder for {Count} To-Do items to user {Username}.")]
  public static partial void LogSendingGroupedReminder(ILogger logger, int count, string username);

  [LoggerMessage(
      EventId = 5006,
      Level = LogLevel.Error,
      Message = "Error processing reminder for To-Do {ToDoId}")]
  public static partial void LogErrorProcessingReminder(ILogger logger, Exception ex, Guid toDoId);

  [LoggerMessage(
      EventId = 5007,
      Level = LogLevel.Information,
      Message = "To-Do Reminder Job completed at {Time}.")]
  public static partial void LogJobCompleted(ILogger logger, DateTimeOffset time);

  [LoggerMessage(
      EventId = 5008,
      Level = LogLevel.Error,
      Message = "To-Do Reminder Job failed with exception:")]
  public static partial void LogJobFailed(ILogger logger, Exception ex);

  [LoggerMessage(
      EventId = 5010,
      Level = LogLevel.Error,
      Message = "Failed to mark reminder {ReminderId} as sent: {ErrorMessage}")]
  public static partial void LogFailedToMarkReminderAsSent(ILogger logger, Guid reminderId, string errorMessage);

  [LoggerMessage(
      EventId = 5011,
      Level = LogLevel.Warning,
      Message = "Failed to unlink reminder {ReminderId} from To-Do {ToDoId}: {ErrorMessage}")]
  public static partial void LogFailedToUnlinkReminder(ILogger logger, Guid reminderId, Guid toDoId, string errorMessage);

  [LoggerMessage(
      EventId = 5012,
      Level = LogLevel.Warning,
      Message = "Failed to delete reminder job {QuartzJobId}: {ErrorMessage}")]
  public static partial void LogFailedToDeleteReminderJob(ILogger logger, Guid quartzJobId, string errorMessage);

  [LoggerMessage(
      EventId = 5013,
      Level = LogLevel.Warning,
      Message = "Failed to delete reminder {ReminderId}: {ErrorMessage}")]
  public static partial void LogFailedToDeleteReminder(ILogger logger, Guid reminderId, string errorMessage);
}
