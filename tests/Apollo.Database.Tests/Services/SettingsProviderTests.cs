using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Apollo.Database.Models;
using Apollo.Database.Services;

namespace Apollo.Database.Tests.Services;

public class SettingsProviderTests : IDisposable
{
    private readonly ApolloDbContext _context;
    private readonly SettingsService _settingsService;
    private readonly SettingsProvider _provider;

    public SettingsProviderTests()
    {
        var options = new DbContextOptionsBuilder<ApolloDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApolloDbContext(options);
        _settingsService = new SettingsService(_context, Mock.Of<ILogger<SettingsService>>());
        _provider = new SettingsProvider(_settingsService, Mock.Of<ILogger<SettingsProvider>>());
    }

    [Fact]
    public async Task ReloadAsync_LoadsSettingsFromDatabase()
    {
        // Arrange
        _context.Settings.Add(new Setting { Key = "daily_alert_channel_id", Value = "12345" });
        _context.Settings.Add(new Setting { Key = "daily_alert_role_id", Value = "67890" });
        _context.Settings.Add(new Setting { Key = "daily_alert_time", Value = "06:00" });
        _context.Settings.Add(new Setting { Key = "daily_alert_initial_message", Value = "Good morning!" });
        _context.Settings.Add(new Setting { Key = "default_timezone", Value = "UTC" });
        _context.Settings.Add(new Setting { Key = "bot_prefix", Value = "!" });
        _context.Settings.Add(new Setting { Key = "debug_logging_enabled", Value = "true" });
        await _context.SaveChangesAsync();

        // Act
        await _provider.ReloadAsync();
        var settings = _provider.GetSettings();

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
    public async Task ReloadAsync_HandlesPartialSettings()
    {
        // Arrange
        _context.Settings.Add(new Setting { Key = "bot_prefix", Value = "?" });
        await _context.SaveChangesAsync();

        // Act
        await _provider.ReloadAsync();
        var settings = _provider.GetSettings();

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
    public async Task ReloadAsync_HandlesEmptyDatabase()
    {
        // Act
        await _provider.ReloadAsync();
        var settings = _provider.GetSettings();

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
    public async Task GetSettings_ReturnsDefaultsBeforeReload()
    {
        // Act
        var settings = _provider.GetSettings();

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
    public async Task ReloadAsync_UpdatesExistingSettings()
    {
        // Arrange
        _context.Settings.Add(new Setting { Key = "bot_prefix", Value = "!" });
        await _context.SaveChangesAsync();

        await _provider.ReloadAsync();
        var initialSettings = _provider.GetSettings();
        Assert.Equal("!", initialSettings.BotPrefix);

        // Act - Update the setting
        var setting = await _context.Settings.FirstAsync(s => s.Key == "bot_prefix");
        setting.Value = "?";
        await _context.SaveChangesAsync();

        await _provider.ReloadAsync();
        var updatedSettings = _provider.GetSettings();

        // Assert
        Assert.Equal("?", updatedSettings.BotPrefix);
    }

    [Fact]
    public async Task ReloadAsync_LoadsDailyAlertTime()
    {
        // Arrange
        _context.Settings.Add(new Setting { Key = "daily_alert_time", Value = "14:30" });
        await _context.SaveChangesAsync();

        // Act
        await _provider.ReloadAsync();
        var settings = _provider.GetSettings();

        // Assert
        Assert.Equal("14:30", settings.DailyAlertTime);
    }

    [Fact]
    public async Task ReloadAsync_LoadsDailyAlertInitialMessage()
    {
        // Arrange
        _context.Settings.Add(new Setting { Key = "daily_alert_initial_message", Value = "What are your goals today?" });
        await _context.SaveChangesAsync();

        // Act
        await _provider.ReloadAsync();
        var settings = _provider.GetSettings();

        // Assert
        Assert.Equal("What are your goals today?", settings.DailyAlertInitialMessage);
    }

    [Fact]
    public async Task ReloadAsync_HandlesMissingDailyAlertSettings()
    {
        // Arrange - Add other settings but not the daily alert time/message
        _context.Settings.Add(new Setting { Key = "bot_prefix", Value = "!" });
        await _context.SaveChangesAsync();

        // Act
        await _provider.ReloadAsync();
        var settings = _provider.GetSettings();

        // Assert
        Assert.Null(settings.DailyAlertTime);
        Assert.Null(settings.DailyAlertInitialMessage);
        Assert.Equal("!", settings.BotPrefix);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
