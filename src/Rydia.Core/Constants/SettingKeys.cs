namespace Rydia.Core.Constants;

/// <summary>
/// Defines the available setting keys that can be stored in the database
/// </summary>
/// <remarks>
/// This class is obsolete. Use IOptions&lt;RydiaSettings&gt; for strongly-typed configuration instead.
/// </remarks>
[Obsolete("Use IOptions<RydiaSettings> from Rydia.Core.Configuration for strongly-typed settings access")]
public static class SettingKeys
{
    /// <summary>
    /// The Discord channel ID where daily alerts are posted
    /// </summary>
    public const string DailyAlertChannelId = "daily_alert_channel_id";

    /// <summary>
    /// The Discord role ID to notify for daily alerts
    /// </summary>
    public const string DailyAlertRoleId = "daily_alert_role_id";

    /// <summary>
    /// Default timezone for scheduled tasks
    /// </summary>
    public const string DefaultTimezone = "default_timezone";

    /// <summary>
    /// Bot prefix for text commands (if any)
    /// </summary>
    public const string BotPrefix = "bot_prefix";

    /// <summary>
    /// Whether debug logging is enabled
    /// </summary>
    public const string DebugLoggingEnabled = "debug_logging_enabled";

    /// <summary>
    /// Returns all valid setting keys
    /// </summary>
    public static readonly IReadOnlyList<string> AllKeys = new List<string>
    {
        DailyAlertChannelId,
        DailyAlertRoleId,
        DefaultTimezone,
        BotPrefix,
        DebugLoggingEnabled
    }.AsReadOnly();

    /// <summary>
    /// Checks if a given key is valid
    /// </summary>
    /// <param name="key">The key to validate</param>
    /// <returns>True if the key is valid, false otherwise</returns>
    public static bool IsValidKey(string key) => AllKeys.Contains(key);
}