namespace Apollo.API.Controllers;

using Apollo.API.Services;
using Apollo.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("/api")]
public partial class ApiController : ControllerBase
{
    private readonly IDiscordMessageSender _discordMessageSender;
    private readonly ILogger<ApiController> _logger;
    private readonly ISettingsProvider _settingsProvider;

    public ApiController(IDiscordMessageSender discordMessageSender, ILogger<ApiController> logger, ISettingsProvider settingsProvider)
    {
        _discordMessageSender = discordMessageSender;
        _logger = logger;
        _settingsProvider = settingsProvider;
    }

    [HttpGet("")]
    public string Ping() => "pong";

    [HttpGet("discord/test-message")]
    public async Task<IActionResult> SendDiscordTestMessage([FromQuery] string? content, CancellationToken ct)
    {
        var body = content ?? $"Apollo test message from API at {DateTimeOffset.UtcNow:O}";

        try
        {
            LogTestMessageStart(_logger, body.Length);
            var channelId = _settingsProvider.GetSettings().DailyAlertChannelId;
            if (channelId == null)
            {
                LogTestMessageBadRequest(_logger, "DailyAlertChannelId is not configured.");
                return BadRequest(new { error = "DailyAlertChannelId is not configured." });
            }

            var (Success, Error, MessageId) = await _discordMessageSender.SendToDailyAlertAsync(body, ct);

            if (!Success)
            {
                // If settings missing, treat as 400; otherwise 502
                if (string.Equals(Error, "DailyAlertChannelId is not configured.", StringComparison.Ordinal))
                {
                    LogTestMessageBadRequest(_logger, Error!);
                    return BadRequest(new { error = Error });
                }

                LogTestMessageFailed(_logger, Error ?? "Unknown error");
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "Discord REST call failed" });
            }

            LogTestMessageSuccess(_logger, MessageId ?? 0UL);
            return Ok(new { channelId = channelId.Value, content = body, messageId = MessageId });
        }
        catch (Exception ex)
        {
            LogTestMessageException(_logger, ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    [HttpPost("discord/forum-post")]
    public async Task<IActionResult> CreateForumPost(
        [FromQuery] ulong? channelId,
        [FromQuery] string? title,
        [FromQuery] string? content,
        [FromQuery] IEnumerable<ulong>? tagIds,
        CancellationToken ct)
    {
        try
        {
            if (channelId is null)
            {
                LogForumPostBadRequest(_logger, "Forum channelId is required.");
                return BadRequest(new { error = "Forum channelId is required." });
            }

            var postTitle = string.IsNullOrWhiteSpace(title)
                ? $"Apollo forum post {DateTimeOffset.UtcNow:O}"
                : title!;

            var body = string.IsNullOrWhiteSpace(content)
                ? $"Apollo forum post created at {DateTimeOffset.UtcNow:O}"
                : content!;

            LogForumPostStart(_logger, channelId.Value, postTitle.Length, body.Length);

            var result = await _discordMessageSender.CreateForumPostAsync(channelId.Value, postTitle, body, tagIds, ct);
            if (!result.Success)
            {
                LogForumPostFailed(_logger, channelId.Value, result.Error ?? "Unknown error");
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "Discord REST call failed" });
            }

            LogForumPostSuccess(_logger, channelId.Value, result.ThreadId ?? 0UL);
            return Ok(new { channelId = channelId.Value, title = postTitle, content = body, threadId = result.ThreadId, messageId = result.MessageId, appliedTagIds = tagIds });
        }
        catch (Exception ex)
        {
            LogForumPostException(_logger, ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    [LoggerMessage(EventId = 200, Level = LogLevel.Information, Message = "Test message endpoint hit with content length {ContentLength}")]
    private static partial void LogTestMessageStart(ILogger logger, int contentLength);

    [LoggerMessage(EventId = 201, Level = LogLevel.Information, Message = "Test message sent successfully. MessageId={MessageId}")]
    private static partial void LogTestMessageSuccess(ILogger logger, ulong messageId);

    [LoggerMessage(EventId = 202, Level = LogLevel.Warning, Message = "Test message bad request: {Error}")]
    private static partial void LogTestMessageBadRequest(ILogger logger, string error);

    [LoggerMessage(EventId = 203, Level = LogLevel.Error, Message = "Test message failed: {Error}")]
    private static partial void LogTestMessageFailed(ILogger logger, string error);

    [LoggerMessage(EventId = 204, Level = LogLevel.Error, Message = "Test message endpoint exception")]
    private static partial void LogTestMessageException(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 210, Level = LogLevel.Information, Message = "Forum post endpoint hit for channel {ChannelId} with title length {TitleLength} and content length {ContentLength}")]
    private static partial void LogForumPostStart(ILogger logger, ulong channelId, int titleLength, int contentLength);

    [LoggerMessage(EventId = 211, Level = LogLevel.Information, Message = "Forum post created successfully in channel {ChannelId}. ThreadId={ThreadId}")]
    private static partial void LogForumPostSuccess(ILogger logger, ulong channelId, ulong threadId);

    [LoggerMessage(EventId = 212, Level = LogLevel.Warning, Message = "Forum post bad request: {Error}")]
    private static partial void LogForumPostBadRequest(ILogger logger, string error);

    [LoggerMessage(EventId = 213, Level = LogLevel.Error, Message = "Forum post failed for channel {ChannelId}: {Error}")]
    private static partial void LogForumPostFailed(ILogger logger, ulong channelId, string error);

    [LoggerMessage(EventId = 214, Level = LogLevel.Error, Message = "Forum post endpoint exception")]
    private static partial void LogForumPostException(ILogger logger, Exception exception);
}
