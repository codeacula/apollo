namespace Apollo.Core.Configuration;

/// <summary>
/// Strongly-typed configuration settings for Rydia
/// </summary>
public class ApolloSettings
{
    /// <summary>
    /// Database key constants for settings
    /// </summary>
    public static class Keys
    {
        public const string DailyAlertChannelId = "daily_alert_channel_id";
        public const string DailyAlertRoleId = "daily_alert_role_id";
        public const string DailyAlertTime = "daily_alert_time";
        public const string DailyAlertInitialMessage = "daily_alert_initial_message";
        public const string DefaultTimezone = "default_timezone";
        public const string BotPrefix = "bot_prefix";
        public const string DebugLoggingEnabled = "debug_logging_enabled";
    }

    /// <summary>
    /// The Discord channel ID where daily alerts are posted
    /// </summary>
    public ulong? DailyAlertChannelId { get; set; }

    /// <summary>
    /// The Discord role ID to notify for daily alerts
    /// </summary>
    public ulong? DailyAlertRoleId { get; set; }

    /// <summary>
    /// The time when daily alerts should be posted (in HH:mm format)
    /// </summary>
    public string? DailyAlertTime { get; set; }

    /// <summary>
    /// The initial message text for daily alerts
    /// </summary>
    public string? DailyAlertInitialMessage { get; set; }

    /// <summary>
    /// Default timezone for scheduled tasks
    /// </summary>
    public string? DefaultTimezone { get; set; }

    /// <summary>
    /// Bot prefix for text commands (if any)
    /// </summary>
    public string? BotPrefix { get; set; }

    /// <summary>
    /// Whether debug logging is enabled
    /// </summary>
    public bool DebugLoggingEnabled { get; set; }
}
