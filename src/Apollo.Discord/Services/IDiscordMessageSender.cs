namespace Apollo.Discord.Services;

public interface IDiscordMessageSender
{
  Task<(bool Success, string? Error, ulong? ThreadId, ulong? MessageId)> CreateForumPostAsync(ulong forumChannelId, string title, string content, IEnumerable<ulong>? appliedTagIds, CancellationToken ct);
}

