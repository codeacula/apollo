using Apollo.Core.Configuration;
using Apollo.Core.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apollo.Database.Services;

public partial class SettingsProvider(ISettingsService settingsService, ILogger<SettingsProvider> logger) : ISettingsProvider
{
  private readonly ISettingsService _settingsService = settingsService;
  private readonly ILogger<SettingsProvider> _logger = logger;
  private ApolloSettings _currentSettings = new();
  private readonly Lock _lock = new();

  public async Task ReloadAsync()
  {
    try
    {
      ApolloSettings settings = new();

      // Load DailyAlertChannelId
      string? channelIdStr = await _settingsService.GetSettingAsync(ApolloSettings.Keys.DailyAlertChannelId);
      if (!string.IsNullOrWhiteSpace(channelIdStr) && ulong.TryParse(channelIdStr, out ulong channelId))
      {
        settings.DailyAlertChannelId = channelId;
      }

      // Load DailyAlertRoleId
      string? roleIdStr = await _settingsService.GetSettingAsync(ApolloSettings.Keys.DailyAlertRoleId);
      if (!string.IsNullOrWhiteSpace(roleIdStr) && ulong.TryParse(roleIdStr, out ulong roleId))
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

public class ApolloSettingsOptions(ISettingsProvider provider) : IOptions<ApolloSettings>
{
  private readonly ISettingsProvider _provider = provider;

  public ApolloSettings Value => _provider.GetSettings();
}
