using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Apollo.Core.Configuration;
using Apollo.Core.Services;

namespace Apollo.Database.Services;

/// <summary>
/// Provides strongly-typed settings from the database
/// </summary>
public partial class SettingsProvider : ISettingsProvider
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsProvider> _logger;
    private ApolloSettings _currentSettings;
    private readonly object _lock = new();

    public SettingsProvider(ISettingsService settingsService, ILogger<SettingsProvider> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
        _currentSettings = new ApolloSettings();
    }

    public async Task ReloadAsync()
    {
        try
        {
            var settings = new ApolloSettings();

            // Load DailyAlertChannelId
            var channelIdStr = await _settingsService.GetSettingAsync(ApolloSettings.Keys.DailyAlertChannelId);
            if (!string.IsNullOrWhiteSpace(channelIdStr) && ulong.TryParse(channelIdStr, out var channelId))
            {
                settings.DailyAlertChannelId = channelId;
            }

            // Load DailyAlertRoleId
            var roleIdStr = await _settingsService.GetSettingAsync(ApolloSettings.Keys.DailyAlertRoleId);
            if (!string.IsNullOrWhiteSpace(roleIdStr) && ulong.TryParse(roleIdStr, out var roleId))
            {
                settings.DailyAlertRoleId = roleId;
            }

            // Load DailyAlertTime
            settings.DailyAlertTime = await _settingsService.GetSettingAsync(ApolloSettings.Keys.DailyAlertTime);

            // Load DailyAlertInitialMessage
            settings.DailyAlertInitialMessage = await _settingsService.GetSettingAsync(ApolloSettings.Keys.DailyAlertInitialMessage);

            // Load DefaultTimezone
            settings.DefaultTimezone = await _settingsService.GetSettingAsync(ApolloSettings.Keys.DefaultTimezone);

            // Load BotPrefix
            settings.BotPrefix = await _settingsService.GetSettingAsync(ApolloSettings.Keys.BotPrefix);

            // Load DebugLoggingEnabled
            settings.DebugLoggingEnabled = await _settingsService.GetBooleanSettingAsync(ApolloSettings.Keys.DebugLoggingEnabled);

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

    public ApolloSettings GetSettings()
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
/// Options wrapper for ApolloSettings that uses the SettingsProvider
/// </summary>
public class ApolloSettingsOptions : IOptions<ApolloSettings>
{
    private readonly ISettingsProvider _provider;

    public ApolloSettingsOptions(ISettingsProvider provider)
    {
        _provider = provider;
    }

    public ApolloSettings Value => _provider.GetSettings();
}
