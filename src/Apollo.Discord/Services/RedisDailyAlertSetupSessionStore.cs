using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Apollo.Discord.Services;

public partial class RedisDailyAlertSetupSessionStore : IDailyAlertSetupSessionStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisDailyAlertSetupSessionStore> _logger;
    private readonly TimeSpan _sessionTtl = TimeSpan.FromMinutes(30);

    public RedisDailyAlertSetupSessionStore(IConnectionMultiplexer redis, ILogger<RedisDailyAlertSetupSessionStore> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    private static string GetKey(ulong guildId, ulong userId) => $"daily_alert_setup:{guildId}:{userId}";

    public async Task<DailyAlertSetupSession?> GetSessionAsync(ulong guildId, ulong userId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetKey(guildId, userId);
            var value = await db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                return null;
            }

            var session = JsonSerializer.Deserialize<DailyAlertSetupSession>(value!, JsonSerializerOptions.Default);
            LogSessionRetrieved(_logger, guildId, userId);
            return session;
        }
        catch (Exception ex)
        {
            LogGetSessionError(_logger, ex, guildId, userId);
            return null;
        }
    }

    public async Task SetSessionAsync(ulong guildId, ulong userId, DailyAlertSetupSession session)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetKey(guildId, userId);
            var value = JsonSerializer.Serialize(session);
            await db.StringSetAsync(key, value, _sessionTtl);
            LogSessionStored(_logger, guildId, userId);
        }
        catch (Exception ex)
        {
            LogSetSessionError(_logger, ex, guildId, userId);
        }
    }

    public async Task DeleteSessionAsync(ulong guildId, ulong userId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetKey(guildId, userId);
            await db.KeyDeleteAsync(key);
            LogSessionDeleted(_logger, guildId, userId);
        }
        catch (Exception ex)
        {
            LogDeleteSessionError(_logger, ex, guildId, userId);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Session retrieved for guild {GuildId}, user {UserId}")]
    private static partial void LogSessionRetrieved(ILogger logger, ulong guildId, ulong userId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Session stored for guild {GuildId}, user {UserId}")]
    private static partial void LogSessionStored(ILogger logger, ulong guildId, ulong userId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Session deleted for guild {GuildId}, user {UserId}")]
    private static partial void LogSessionDeleted(ILogger logger, ulong guildId, ulong userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving session for guild {GuildId}, user {UserId}")]
    private static partial void LogGetSessionError(ILogger logger, Exception exception, ulong guildId, ulong userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error storing session for guild {GuildId}, user {UserId}")]
    private static partial void LogSetSessionError(ILogger logger, Exception exception, ulong guildId, ulong userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error deleting session for guild {GuildId}, user {UserId}")]
    private static partial void LogDeleteSessionError(ILogger logger, Exception exception, ulong guildId, ulong userId);
}
