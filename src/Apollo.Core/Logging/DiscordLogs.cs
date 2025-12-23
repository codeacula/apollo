using Microsoft.Extensions.Logging;

namespace Apollo.Core.Logging;

/// <summary>
/// High-performance logging definitions for Discord operations.
/// EventIds: 2000-2099
/// </summary>
public static partial class DiscordLogs
{
  [LoggerMessage(
      EventId = 2000,
      Level = LogLevel.Information,
      Message = "Attempting to send Discord message to channel {ChannelId}.")]
  public static partial void SendingMessage(ILogger logger, ulong channelId);

  [LoggerMessage(
      EventId = 2001,
      Level = LogLevel.Information,
      Message = "Successfully sent Discord message {MessageId} to channel {ChannelId}.")]
  public static partial void MessageSent(ILogger logger, ulong channelId, ulong messageId);

  [LoggerMessage(
      EventId = 2002,
      Level = LogLevel.Error,
      Message = "Failed to send Discord message to channel {ChannelId}: {Error}")]
  public static partial void MessageSendFailed(ILogger logger, ulong channelId, string error);

  [LoggerMessage(
      EventId = 2003,
      Level = LogLevel.Information,
      Message = "Creating forum post in channel {ChannelId} with title '{Title}'.")]
  public static partial void CreatingForumPost(ILogger logger, ulong channelId, string title);

  [LoggerMessage(
      EventId = 2004,
      Level = LogLevel.Information,
      Message = "Forum post created in channel {ChannelId} with thread id {ThreadId}.")]
  public static partial void ForumPostCreated(ILogger logger, ulong channelId, ulong threadId);

  [LoggerMessage(
      EventId = 2005,
      Level = LogLevel.Error,
      Message = "Failed to create forum post in channel {ChannelId}: {Error}")]
  public static partial void ForumPostCreateFailed(ILogger logger, ulong channelId, string error);

  [LoggerMessage(
      EventId = 2006,
      Level = LogLevel.Error,
      Message = "Exception occurred while processing incoming message from user {Username}: {ExceptionMessage}")]
  public static partial void MessageProcessingFailed(ILogger logger, string username, string exceptionMessage, Exception exception);
}
