namespace Rydia.Core.Configuration;

/// <summary>
/// Strongly-typed configuration settings for Rydia
/// </summary>
public class RydiaSettings
{
    /// <summary>
    /// The Discord channel ID where daily alerts are posted
    /// </summary>
    public ulong? DailyAlertChannelId { get; set; }

    /// <summary>
    /// The Discord role ID to notify for daily alerts
    /// </summary>
    public ulong? DailyAlertRoleId { get; set; }

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
