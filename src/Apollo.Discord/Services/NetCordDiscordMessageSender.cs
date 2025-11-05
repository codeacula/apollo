using NetCord.Rest;

namespace Apollo.Discord.Services;

public class NetCordDiscordMessageSender(RestClient restClient, ILogger<NetCordDiscordMessageSender> logger) : IDiscordMessageSender
{
  private readonly RestClient _restClient = restClient;
  private readonly ILogger<NetCordDiscordMessageSender> _logger = logger;

  public async Task<(bool Success, string? Error, ulong? ThreadId, ulong? MessageId)> CreateForumPostAsync(ulong forumChannelId, string title, string content, IEnumerable<ulong>? appliedTagIds, CancellationToken ct)
  {
    try
    {
      Log.CreatingForumPost(_logger, forumChannelId, title);
      ForumGuildThreadMessageProperties message = new() { Content = content };
      ForumGuildThreadProperties threadProps = new ForumGuildThreadProperties(title, message)
          .WithAppliedTags(appliedTagIds);

      NetCord.ForumGuildThread thread = await _restClient.CreateForumGuildThreadAsync(forumChannelId, threadProps, null, ct);
      Log.ForumPostCreated(_logger, forumChannelId, thread.Id);

      // For a newly created forum post, the initial message ID is the thread's last message id if available.
      // NetCord ForumGuildThread does not directly expose initial message id; returning null for MessageId is acceptable.
      return (true, null, thread.Id, null);
    }
    catch (Exception ex)
    {
      Log.ForumPostCreateFailed(_logger, forumChannelId, ex.Message);
      return (false, "Discord REST call failed.", null, null);
    }
  }
}
public static partial class Log
{
  [LoggerMessage(EventId = 100, Level = LogLevel.Information, Message = "Attempting to send Discord message to channel {ChannelId}.")]
  public static partial void SendingMessage(ILogger logger, ulong channelId);

  [LoggerMessage(EventId = 101, Level = LogLevel.Information, Message = "Successfully sent Discord message {MessageId} to channel {ChannelId}.")]
  public static partial void MessageSent(ILogger logger, ulong channelId, ulong messageId);

  [LoggerMessage(EventId = 102, Level = LogLevel.Error, Message = "Failed to send Discord message to channel {ChannelId}: {Error}")]
  public static partial void MessageSendFailed(ILogger logger, ulong channelId, string error);

  [LoggerMessage(EventId = 110, Level = LogLevel.Information, Message = "Creating forum post in channel {ChannelId} with title '{Title}'.")]
  public static partial void CreatingForumPost(ILogger logger, ulong channelId, string title);

  [LoggerMessage(EventId = 111, Level = LogLevel.Information, Message = "Forum post created in channel {ChannelId} with thread id {ThreadId}.")]
  public static partial void ForumPostCreated(ILogger logger, ulong channelId, ulong threadId);

  [LoggerMessage(EventId = 112, Level = LogLevel.Error, Message = "Failed to create forum post in channel {ChannelId}: {Error}")]
  public static partial void ForumPostCreateFailed(ILogger logger, ulong channelId, string error);
}
