
using Apollo.Database.Models;
using Apollo.Database.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace Apollo.Database.Tests.Services;

public class SettingsProviderTests : IDisposable
{
  private readonly ApolloDbContext _context;
  private readonly SettingsService _settingsService;
  private readonly SettingsProvider _provider;

  public SettingsProviderTests()
  {
    DbContextOptions<ApolloDbContext> options = new DbContextOptionsBuilder<ApolloDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new ApolloDbContext(options);
    _settingsService = new SettingsService(_context, Mock.Of<ILogger<SettingsService>>());
    _provider = new SettingsProvider(_settingsService, Mock.Of<ILogger<SettingsProvider>>());
  }

  [Fact]
  public async Task ReloadAsyncLoadsSettingsFromDatabaseAsync()
  {
    // Arrange
    _ = _context.Settings.Add(new Setting { Key = "daily_alert_channel_id", Value = "12345" });
    _ = _context.Settings.Add(new Setting { Key = "daily_alert_role_id", Value = "67890" });
    _ = _context.Settings.Add(new Setting { Key = "daily_alert_time", Value = "06:00" });
    _ = _context.Settings.Add(new Setting { Key = "daily_alert_initial_message", Value = "Good morning!" });
    _ = _context.Settings.Add(new Setting { Key = "default_timezone", Value = "UTC" });
    _ = _context.Settings.Add(new Setting { Key = "bot_prefix", Value = "!" });
    _ = _context.Settings.Add(new Setting { Key = "debug_logging_enabled", Value = "true" });
    _ = await _context.SaveChangesAsync();

    // Act
    await _provider.ReloadAsync();
    Core.Configuration.ApolloSettings settings = _provider.GetSettings();

    // Assert
    Assert.Equal(12345ul, settings.DailyAlertChannelId);
    Assert.Equal(67890ul, settings.DailyAlertRoleId);
    Assert.Equal("06:00", settings.DailyAlertTime);
    Assert.Equal("Good morning!", settings.DailyAlertInitialMessage);
    Assert.Equal("UTC", settings.DefaultTimezone);
    Assert.Equal("!", settings.BotPrefix);
    Assert.True(settings.DebugLoggingEnabled);
  }

  [Fact]
  public async Task ReloadAsyncHandlesPartialSettingsAsync()
  {
    // Arrange
    _ = _context.Settings.Add(new Setting { Key = "bot_prefix", Value = "?" });
    _ = await _context.SaveChangesAsync();

    // Act
    await _provider.ReloadAsync();
    Core.Configuration.ApolloSettings settings = _provider.GetSettings();

    // Assert
    Assert.Null(settings.DailyAlertChannelId);
    Assert.Null(settings.DailyAlertRoleId);
    Assert.Null(settings.DailyAlertTime);
    Assert.Null(settings.DailyAlertInitialMessage);
    Assert.Null(settings.DefaultTimezone);
    Assert.Equal("?", settings.BotPrefix);
    Assert.False(settings.DebugLoggingEnabled);
  }

  [Fact]
  public async Task ReloadAsyncHandlesEmptyDatabaseAsync()
  {
    // Act
    await _provider.ReloadAsync();
    Core.Configuration.ApolloSettings settings = _provider.GetSettings();

    // Assert
    Assert.Null(settings.DailyAlertChannelId);
    Assert.Null(settings.DailyAlertRoleId);
    Assert.Null(settings.DailyAlertTime);
    Assert.Null(settings.DailyAlertInitialMessage);
    Assert.Null(settings.DefaultTimezone);
    Assert.Null(settings.BotPrefix);
    Assert.False(settings.DebugLoggingEnabled);
  }

  [Fact]
  public void GetSettingsReturnsDefaultsBeforeReload()
  {
    // Act
    Core.Configuration.ApolloSettings settings = _provider.GetSettings();

    // Assert
    Assert.Null(settings.DailyAlertChannelId);
    Assert.Null(settings.DailyAlertRoleId);
    Assert.Null(settings.DailyAlertTime);
    Assert.Null(settings.DailyAlertInitialMessage);
    Assert.Null(settings.DefaultTimezone);
    Assert.Null(settings.BotPrefix);
    Assert.False(settings.DebugLoggingEnabled);
  }

  [Fact]
  public async Task ReloadAsyncUpdatesExistingSettingsAsync()
  {
    // Arrange
    _ = _context.Settings.Add(new Setting { Key = "bot_prefix", Value = "!" });
    _ = await _context.SaveChangesAsync();

    await _provider.ReloadAsync();
    Core.Configuration.ApolloSettings initialSettings = _provider.GetSettings();
    Assert.Equal("!", initialSettings.BotPrefix);

    // Act - Update the setting
    Setting setting = await _context.Settings.FirstAsync(s => s.Key == "bot_prefix");
    setting.Value = "?";
    _ = await _context.SaveChangesAsync();

    await _provider.ReloadAsync();
    Core.Configuration.ApolloSettings updatedSettings = _provider.GetSettings();

    // Assert
    Assert.Equal("?", updatedSettings.BotPrefix);
  }

  [Fact]
  public async Task ReloadAsyncLoadsDailyAlertTimeAsync()
  {
    // Arrange
    _ = _context.Settings.Add(new Setting { Key = "daily_alert_time", Value = "14:30" });
    _ = await _context.SaveChangesAsync();

    // Act
    await _provider.ReloadAsync();
    Core.Configuration.ApolloSettings settings = _provider.GetSettings();

    // Assert
    Assert.Equal("14:30", settings.DailyAlertTime);
  }

  [Fact]
  public async Task ReloadAsyncLoadsDailyAlertInitialMessageAsync()
  {
    // Arrange
    _ = _context.Settings.Add(new Setting { Key = "daily_alert_initial_message", Value = "What are your goals today?" });
    _ = await _context.SaveChangesAsync();

    // Act
    await _provider.ReloadAsync();
    Core.Configuration.ApolloSettings settings = _provider.GetSettings();

    // Assert
    Assert.Equal("What are your goals today?", settings.DailyAlertInitialMessage);
  }

  [Fact]
  public async Task ReloadAsyncHandlesMissingDailyAlertSettingsAsync()
  {
    // Arrange - Add other settings but not the daily alert time/message
    _ = _context.Settings.Add(new Setting { Key = "bot_prefix", Value = "!" });
    _ = await _context.SaveChangesAsync();

    // Act
    await _provider.ReloadAsync();
    Core.Configuration.ApolloSettings settings = _provider.GetSettings();

    // Assert
    Assert.Null(settings.DailyAlertTime);
    Assert.Null(settings.DailyAlertInitialMessage);
    Assert.Equal("!", settings.BotPrefix);
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    _context.Dispose();
  }
}
