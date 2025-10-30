
using Apollo.Core.Configuration;
using Apollo.Database.Models;
using Apollo.Database.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace Apollo.Database.Tests.Services;

public class SettingsServiceTests : IDisposable
{
  private readonly ApolloDbContext _context;
  private readonly Mock<ILogger<SettingsService>> _loggerMock;
  private readonly SettingsService _service;

  public SettingsServiceTests()
  {
    DbContextOptions<ApolloDbContext> options = new DbContextOptionsBuilder<ApolloDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new ApolloDbContext(options);
    _loggerMock = new Mock<ILogger<SettingsService>>();
    _service = new SettingsService(_context, _loggerMock.Object);
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    _context.Dispose();
  }

  [Fact]
  public async Task GetSettingAsyncWithNonExistentKeyReturnsNullAsync()
  {
    string? result = await _service.GetSettingAsync(ApolloSettings.Keys.BotPrefix);

    Assert.Null(result);
  }

  [Fact]
  public async Task GetSettingAsyncWithNullKeyReturnsNullAsync()
  {
    string? result = await _service.GetSettingAsync(null!);

    Assert.Null(result);
  }

  [Fact]
  public async Task GetSettingAsyncWithEmptyKeyReturnsNullAsync()
  {
    string? result = await _service.GetSettingAsync("");

    Assert.Null(result);
  }

  [Fact]
  public async Task GetSettingAsyncWithInvalidKeyReturnsNullAsync()
  {
    string? result = await _service.GetSettingAsync("invalid_key");

    Assert.Null(result);
  }

  [Fact]
  public async Task GetBooleanSettingAsyncWithTrueValueReturnsTrueAsync()
  {
    Setting setting = new() { Key = ApolloSettings.Keys.DebugLoggingEnabled, Value = "true" };
    _ = _context.Settings.Add(setting);
    _ = await _context.SaveChangesAsync();

    bool result = await _service.GetBooleanSettingAsync(ApolloSettings.Keys.DebugLoggingEnabled);

    Assert.True(result);
  }

  [Fact]
  public async Task GetBooleanSettingAsyncWithFalseValueReturnsFalseAsync()
  {
    Setting setting = new() { Key = ApolloSettings.Keys.DebugLoggingEnabled, Value = "false" };
    _ = _context.Settings.Add(setting);
    _ = await _context.SaveChangesAsync();

    bool result = await _service.GetBooleanSettingAsync(ApolloSettings.Keys.DebugLoggingEnabled);

    Assert.False(result);
  }

  [Theory]
  [InlineData("1", true)]
  [InlineData("yes", true)]
  [InlineData("on", true)]
  [InlineData("enabled", true)]
  [InlineData("0", false)]
  [InlineData("no", false)]
  [InlineData("off", false)]
  public async Task GetBooleanSettingAsyncWithAlternativeValuesReturnsCorrectResultAsync(string value, bool expected)
  {
    Setting setting = new() { Key = ApolloSettings.Keys.DebugLoggingEnabled, Value = value };
    _ = _context.Settings.Add(setting);
    _ = await _context.SaveChangesAsync();

    bool result = await _service.GetBooleanSettingAsync(ApolloSettings.Keys.DebugLoggingEnabled);

    Assert.Equal(expected, result);
  }

  [Fact]
  public async Task GetBooleanSettingAsyncWithNonExistentKeyReturnsDefaultAsync()
  {
    bool result = await _service.GetBooleanSettingAsync(ApolloSettings.Keys.DebugLoggingEnabled, true);

    Assert.True(result);
  }

  [Fact]
  public async Task GetIntegerSettingAsyncWithValidValueReturnsIntegerAsync()
  {
    Setting setting = new() { Key = ApolloSettings.Keys.DefaultTimezone, Value = "42" };
    _ = _context.Settings.Add(setting);
    _ = await _context.SaveChangesAsync();

    int result = await _service.GetIntegerSettingAsync(ApolloSettings.Keys.DefaultTimezone);

    Assert.Equal(42, result);
  }

  [Fact]
  public async Task GetIntegerSettingAsyncWithInvalidValueReturnsDefaultAsync()
  {
    Setting setting = new() { Key = ApolloSettings.Keys.DefaultTimezone, Value = "not_a_number" };
    _ = _context.Settings.Add(setting);
    _ = await _context.SaveChangesAsync();

    int result = await _service.GetIntegerSettingAsync(ApolloSettings.Keys.DefaultTimezone, 10);

    Assert.Equal(10, result);
  }

  [Fact]
  public async Task GetIntegerSettingAsyncWithNonExistentKeyReturnsDefaultAsync()
  {
    int result = await _service.GetIntegerSettingAsync(ApolloSettings.Keys.DefaultTimezone, 5);

    Assert.Equal(5, result);
  }

  [Fact]
  public async Task SetSettingAsyncWithNewKeyCreatesNewSettingAsync()
  {
    bool result = await _service.SetSettingAsync(ApolloSettings.Keys.BotPrefix, "!");

    Assert.True(result);
    Setting? savedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == ApolloSettings.Keys.BotPrefix);
    Assert.NotNull(savedSetting);
    Assert.Equal("!", savedSetting.Value);
  }

  [Fact]
  public async Task SetSettingAsyncWithExistingKeyUpdatesSettingAsync()
  {
    Setting setting = new() { Key = ApolloSettings.Keys.BotPrefix, Value = "!" };
    _ = _context.Settings.Add(setting);
    _ = await _context.SaveChangesAsync();

    bool result = await _service.SetSettingAsync(ApolloSettings.Keys.BotPrefix, "?");

    Assert.True(result);
    Setting? updatedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == ApolloSettings.Keys.BotPrefix);
    Assert.NotNull(updatedSetting);
    Assert.Equal("?", updatedSetting.Value);
  }

  [Fact]
  public async Task SetSettingAsyncWithNullKeyReturnsFalseAsync()
  {
    bool result = await _service.SetSettingAsync(null!, "value");

    Assert.False(result);
  }

  [Fact]
  public async Task SetSettingAsyncWithEmptyKeyReturnsFalseAsync()
  {
    bool result = await _service.SetSettingAsync("", "value");

    Assert.False(result);
  }

  [Fact]
  public async Task SetSettingAsyncWithAnyKeySucceedsAsync()
  {
    // After IOptions migration, any key is allowed (no longer restricted by SettingKeys)
    bool result = await _service.SetSettingAsync("custom_key", "value");

    Assert.True(result);
    Setting? savedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == "custom_key");
    Assert.NotNull(savedSetting);
    Assert.Equal("value", savedSetting.Value);
  }

  [Fact]
  public async Task SetSettingAsyncWithNullValueReturnsFalseAsync()
  {
    bool result = await _service.SetSettingAsync(ApolloSettings.Keys.BotPrefix, null!);

    Assert.False(result);
  }

  [Fact]
  public async Task SetBooleanSettingAsyncWithTrueSavesLowercaseTrueAsync()
  {
    bool result = await _service.SetBooleanSettingAsync(ApolloSettings.Keys.DebugLoggingEnabled, true);

    Assert.True(result);
    Setting? savedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == ApolloSettings.Keys.DebugLoggingEnabled);
    Assert.NotNull(savedSetting);
    Assert.Equal("true", savedSetting.Value);
  }

  [Fact]
  public async Task SetBooleanSettingAsyncWithFalseSavesLowercaseFalseAsync()
  {
    bool result = await _service.SetBooleanSettingAsync(ApolloSettings.Keys.DebugLoggingEnabled, false);

    Assert.True(result);
    Setting? savedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == ApolloSettings.Keys.DebugLoggingEnabled);
    Assert.NotNull(savedSetting);
    Assert.Equal("false", savedSetting.Value);
  }

  [Fact]
  public async Task SetIntegerSettingAsyncSavesValueAsStringAsync()
  {
    bool result = await _service.SetIntegerSettingAsync(ApolloSettings.Keys.DefaultTimezone, 123);

    Assert.True(result);
    Setting? savedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == ApolloSettings.Keys.DefaultTimezone);
    Assert.NotNull(savedSetting);
    Assert.Equal("123", savedSetting.Value);
  }

  [Fact]
  public async Task GetAllSettingsAsyncReturnsAllSettingsAsync()
  {
    _ = _context.Settings.Add(new Setting { Key = ApolloSettings.Keys.BotPrefix, Value = "!" });
    _ = _context.Settings.Add(new Setting { Key = ApolloSettings.Keys.DefaultTimezone, Value = "UTC" });
    _ = await _context.SaveChangesAsync();

    Dictionary<string, string> result = await _service.GetAllSettingsAsync();

    Assert.Equal(2, result.Count);
    Assert.Equal("!", result[ApolloSettings.Keys.BotPrefix]);
    Assert.Equal("UTC", result[ApolloSettings.Keys.DefaultTimezone]);
  }

  [Fact]
  public async Task GetAllSettingsAsyncWithNoSettingsReturnsEmptyDictionaryAsync()
  {
    Dictionary<string, string> result = await _service.GetAllSettingsAsync();

    Assert.Empty(result);
  }

  [Fact]
  public async Task DeleteSettingAsyncWithExistingKeyDeletesSettingAsync()
  {
    Setting setting = new() { Key = ApolloSettings.Keys.BotPrefix, Value = "!" };
    _ = _context.Settings.Add(setting);
    _ = await _context.SaveChangesAsync();

    bool result = await _service.DeleteSettingAsync(ApolloSettings.Keys.BotPrefix);

    Assert.True(result);
    Setting? deletedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == ApolloSettings.Keys.BotPrefix);
    Assert.Null(deletedSetting);
  }

  [Fact]
  public async Task DeleteSettingAsyncWithNonExistentKeyReturnsFalseAsync()
  {
    bool result = await _service.DeleteSettingAsync(ApolloSettings.Keys.BotPrefix);

    Assert.False(result);
  }

  [Fact]
  public async Task DeleteSettingAsyncWithNullKeyReturnsFalseAsync()
  {
    bool result = await _service.DeleteSettingAsync(null!);

    Assert.False(result);
  }

  [Fact]
  public async Task DeleteSettingAsyncWithEmptyKeyReturnsFalseAsync()
  {
    bool result = await _service.DeleteSettingAsync("");

    Assert.False(result);
  }

  [Fact]
  public async Task SettingExistsAsyncWithExistingKeyReturnsTrueAsync()
  {
    Setting setting = new() { Key = ApolloSettings.Keys.BotPrefix, Value = "!" };
    _ = _context.Settings.Add(setting);
    _ = await _context.SaveChangesAsync();

    bool result = await _service.SettingExistsAsync(ApolloSettings.Keys.BotPrefix);

    Assert.True(result);
  }

  [Fact]
  public async Task SettingExistsAsyncWithNonExistentKeyReturnsFalseAsync()
  {
    bool result = await _service.SettingExistsAsync(ApolloSettings.Keys.BotPrefix);

    Assert.False(result);
  }

  [Fact]
  public async Task SettingExistsAsyncWithNullKeyReturnsFalseAsync()
  {
    bool result = await _service.SettingExistsAsync(null!);

    Assert.False(result);
  }

  [Fact]
  public async Task SettingExistsAsyncWithEmptyKeyReturnsFalseAsync()
  {
    bool result = await _service.SettingExistsAsync("");

    Assert.False(result);
  }
}
