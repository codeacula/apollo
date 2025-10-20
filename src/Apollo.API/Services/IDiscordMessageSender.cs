namespace Apollo.API.Services;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Abstraction for sending Discord messages to the daily alert channel.
/// </summary>
public interface IDiscordMessageSender
{
    /// <summary>
    /// Sends a message to the configured daily alert Discord channel.
    /// </summary>
    /// <param name="content">The message content.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Tuple indicating success, error message, and Discord message ID.</returns>
    Task<(bool Success, string? Error, ulong? MessageId)> SendToDailyAlertAsync(string content, CancellationToken ct);

    /// <summary>
    /// Creates a new post (thread) in a Discord forum channel.
    /// </summary>
    /// <param name="forumChannelId">The ID of the forum channel where to create the post.</param>
    /// <param name="title">The thread title.</param>
    /// <param name="content">The initial message content for the thread.</param>
    /// <param name="appliedTagIds">Optional tag IDs to apply to the thread.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Tuple indicating success, error message, created thread ID and initial message ID.</returns>
    Task<(bool Success, string? Error, ulong? ThreadId, ulong? MessageId)> CreateForumPostAsync(ulong forumChannelId, string title, string content, IEnumerable<ulong>? appliedTagIds, CancellationToken ct);
}

