using Apollo.Core.Logging;

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
      DiscordLogs.CreatingForumPost(_logger, forumChannelId, title);
      ForumGuildThreadMessageProperties message = new() { Content = content };
      ForumGuildThreadProperties threadProps = new ForumGuildThreadProperties(title, message)
          .WithAppliedTags(appliedTagIds);

      NetCord.ForumGuildThread thread = await _restClient.CreateForumGuildThreadAsync(forumChannelId, threadProps, null, ct);
      DiscordLogs.ForumPostCreated(_logger, forumChannelId, thread.Id);

      // For a newly created forum post, the initial message ID is the thread's last message id if available.
      // NetCord ForumGuildThread does not directly expose initial message id; returning null for MessageId is acceptable.
      return (true, null, thread.Id, null);
    }
    catch (Exception ex)
    {
      DiscordLogs.ForumPostCreateFailed(_logger, forumChannelId, ex.Message);
      return (false, "Discord REST call failed.", null, null);
    }
  }
}
