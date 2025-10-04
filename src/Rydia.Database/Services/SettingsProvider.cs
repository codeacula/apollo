using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rydia.Core.Configuration;
using Rydia.Core.Services;

namespace Rydia.Database.Services;

/// <summary>
/// Provides strongly-typed settings from the database
/// </summary>
public partial class SettingsProvider : ISettingsProvider
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsProvider> _logger;
    private RydiaSettings _currentSettings;
    private readonly object _lock = new();

    public SettingsProvider(ISettingsService settingsService, ILogger<SettingsProvider> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
        _currentSettings = new RydiaSettings();
    }

    public async Task ReloadAsync()
    {
        try
        {
            var settings = new RydiaSettings();

            // Load DailyAlertChannelId
            var channelIdStr = await _settingsService.GetSettingAsync(RydiaSettings.Keys.DailyAlertChannelId);
            if (!string.IsNullOrWhiteSpace(channelIdStr) && ulong.TryParse(channelIdStr, out var channelId))
            {
                settings.DailyAlertChannelId = channelId;
            }

            // Load DailyAlertRoleId
            var roleIdStr = await _settingsService.GetSettingAsync(RydiaSettings.Keys.DailyAlertRoleId);
            if (!string.IsNullOrWhiteSpace(roleIdStr) && ulong.TryParse(roleIdStr, out var roleId))
            {
                settings.DailyAlertRoleId = roleId;
            }

            // Load DailyAlertTime
            settings.DailyAlertTime = await _settingsService.GetSettingAsync(RydiaSettings.Keys.DailyAlertTime);

            // Load DailyAlertInitialMessage
            settings.DailyAlertInitialMessage = await _settingsService.GetSettingAsync(RydiaSettings.Keys.DailyAlertInitialMessage);

            // Load DefaultTimezone
            settings.DefaultTimezone = await _settingsService.GetSettingAsync(RydiaSettings.Keys.DefaultTimezone);

            // Load BotPrefix
            settings.BotPrefix = await _settingsService.GetSettingAsync(RydiaSettings.Keys.BotPrefix);

            // Load DebugLoggingEnabled
            settings.DebugLoggingEnabled = await _settingsService.GetBooleanSettingAsync(RydiaSettings.Keys.DebugLoggingEnabled);

            lock (_lock)
            {
                _currentSettings = settings;
            }

            LogSettingsReloaded(_logger);
        }
        catch (Exception ex)
        {
            LogErrorReloadingSettings(_logger, ex);
        }
    }

    public RydiaSettings GetSettings()
    {
        lock (_lock)
        {
            return _currentSettings;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Settings reloaded from database")]
    private static partial void LogSettingsReloaded(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error reloading settings from database")]
    private static partial void LogErrorReloadingSettings(ILogger logger, Exception exception);
}

/// <summary>
/// Options wrapper for RydiaSettings that uses the SettingsProvider
/// </summary>
public class RydiaSettingsOptions : IOptions<RydiaSettings>
{
    private readonly ISettingsProvider _provider;

    public RydiaSettingsOptions(ISettingsProvider provider)
    {
        _provider = provider;
    }

    public RydiaSettings Value => _provider.GetSettings();
}
