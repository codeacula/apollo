using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Rydia.Core.Constants;
using Rydia.Database.Models;
using Rydia.Database.Services;

namespace Rydia.Database.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly RydiaDbContext _context;
    private readonly Mock<ILogger<SettingsService>> _loggerMock;
    private readonly SettingsService _service;

    public SettingsServiceTests()
    {
        var options = new DbContextOptionsBuilder<RydiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RydiaDbContext(options);
        _loggerMock = new Mock<ILogger<SettingsService>>();
        _service = new SettingsService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetSettingAsync_WithValidKey_ReturnsValue()
    {
        var setting = new Setting { Key = SettingKeys.BotPrefix, Value = "!" };
        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();

        var result = await _service.GetSettingAsync(SettingKeys.BotPrefix);

        Assert.Equal("!", result);
    }

    [Fact]
    public async Task GetSettingAsync_WithNonExistentKey_ReturnsNull()
    {
        var result = await _service.GetSettingAsync(SettingKeys.BotPrefix);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSettingAsync_WithNullKey_ReturnsNull()
    {
        var result = await _service.GetSettingAsync(null!);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSettingAsync_WithEmptyKey_ReturnsNull()
    {
        var result = await _service.GetSettingAsync("");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSettingAsync_WithInvalidKey_ReturnsNull()
    {
        var result = await _service.GetSettingAsync("invalid_key");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBooleanSettingAsync_WithTrueValue_ReturnsTrue()
    {
        var setting = new Setting { Key = SettingKeys.DebugLoggingEnabled, Value = "true" };
        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();

        var result = await _service.GetBooleanSettingAsync(SettingKeys.DebugLoggingEnabled);

        Assert.True(result);
    }

    [Fact]
    public async Task GetBooleanSettingAsync_WithFalseValue_ReturnsFalse()
    {
        var setting = new Setting { Key = SettingKeys.DebugLoggingEnabled, Value = "false" };
        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();

        var result = await _service.GetBooleanSettingAsync(SettingKeys.DebugLoggingEnabled);

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
    public async Task GetBooleanSettingAsync_WithAlternativeValues_ReturnsCorrectResult(string value, bool expected)
    {
        var setting = new Setting { Key = SettingKeys.DebugLoggingEnabled, Value = value };
        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();

        var result = await _service.GetBooleanSettingAsync(SettingKeys.DebugLoggingEnabled);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetBooleanSettingAsync_WithNonExistentKey_ReturnsDefault()
    {
        var result = await _service.GetBooleanSettingAsync(SettingKeys.DebugLoggingEnabled, true);

        Assert.True(result);
    }

    [Fact]
    public async Task GetIntegerSettingAsync_WithValidValue_ReturnsInteger()
    {
        var setting = new Setting { Key = SettingKeys.DefaultTimezone, Value = "42" };
        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();

        var result = await _service.GetIntegerSettingAsync(SettingKeys.DefaultTimezone);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task GetIntegerSettingAsync_WithInvalidValue_ReturnsDefault()
    {
        var setting = new Setting { Key = SettingKeys.DefaultTimezone, Value = "not_a_number" };
        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();

        var result = await _service.GetIntegerSettingAsync(SettingKeys.DefaultTimezone, 10);

        Assert.Equal(10, result);
    }

    [Fact]
    public async Task GetIntegerSettingAsync_WithNonExistentKey_ReturnsDefault()
    {
        var result = await _service.GetIntegerSettingAsync(SettingKeys.DefaultTimezone, 5);

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task SetSettingAsync_WithNewKey_CreatesNewSetting()
    {
        var result = await _service.SetSettingAsync(SettingKeys.BotPrefix, "!");

        Assert.True(result);
        var savedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == SettingKeys.BotPrefix);
        Assert.NotNull(savedSetting);
        Assert.Equal("!", savedSetting.Value);
    }

    [Fact]
    public async Task SetSettingAsync_WithExistingKey_UpdatesSetting()
    {
        var setting = new Setting { Key = SettingKeys.BotPrefix, Value = "!" };
        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();

        var result = await _service.SetSettingAsync(SettingKeys.BotPrefix, "?");

        Assert.True(result);
        var updatedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == SettingKeys.BotPrefix);
        Assert.NotNull(updatedSetting);
        Assert.Equal("?", updatedSetting.Value);
    }

    [Fact]
    public async Task SetSettingAsync_WithNullKey_ReturnsFalse()
    {
        var result = await _service.SetSettingAsync(null!, "value");

        Assert.False(result);
    }

    [Fact]
    public async Task SetSettingAsync_WithEmptyKey_ReturnsFalse()
    {
        var result = await _service.SetSettingAsync("", "value");

        Assert.False(result);
    }

    [Fact]
    public async Task SetSettingAsync_WithAnyKey_Succeeds()
    {
        // After IOptions migration, any key is allowed (no longer restricted by SettingKeys)
        var result = await _service.SetSettingAsync("custom_key", "value");

        Assert.True(result);
        var savedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == "custom_key");
        Assert.NotNull(savedSetting);
        Assert.Equal("value", savedSetting.Value);
    }

    [Fact]
    public async Task SetSettingAsync_WithNullValue_ReturnsFalse()
    {
        var result = await _service.SetSettingAsync(SettingKeys.BotPrefix, null!);

        Assert.False(result);
    }

    [Fact]
    public async Task SetBooleanSettingAsync_WithTrue_SavesLowercaseTrue()
    {
        var result = await _service.SetBooleanSettingAsync(SettingKeys.DebugLoggingEnabled, true);

        Assert.True(result);
        var savedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == SettingKeys.DebugLoggingEnabled);
        Assert.NotNull(savedSetting);
        Assert.Equal("true", savedSetting.Value);
    }

    [Fact]
    public async Task SetBooleanSettingAsync_WithFalse_SavesLowercaseFalse()
    {
        var result = await _service.SetBooleanSettingAsync(SettingKeys.DebugLoggingEnabled, false);

        Assert.True(result);
        var savedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == SettingKeys.DebugLoggingEnabled);
        Assert.NotNull(savedSetting);
        Assert.Equal("false", savedSetting.Value);
    }

    [Fact]
    public async Task SetIntegerSettingAsync_SavesValueAsString()
    {
        var result = await _service.SetIntegerSettingAsync(SettingKeys.DefaultTimezone, 123);

        Assert.True(result);
        var savedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == SettingKeys.DefaultTimezone);
        Assert.NotNull(savedSetting);
        Assert.Equal("123", savedSetting.Value);
    }

    [Fact]
    public async Task GetAllSettingsAsync_ReturnsAllSettings()
    {
        _context.Settings.Add(new Setting { Key = SettingKeys.BotPrefix, Value = "!" });
        _context.Settings.Add(new Setting { Key = SettingKeys.DefaultTimezone, Value = "UTC" });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllSettingsAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("!", result[SettingKeys.BotPrefix]);
        Assert.Equal("UTC", result[SettingKeys.DefaultTimezone]);
    }

    [Fact]
    public async Task GetAllSettingsAsync_WithNoSettings_ReturnsEmptyDictionary()
    {
        var result = await _service.GetAllSettingsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteSettingAsync_WithExistingKey_DeletesSetting()
    {
        var setting = new Setting { Key = SettingKeys.BotPrefix, Value = "!" };
        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteSettingAsync(SettingKeys.BotPrefix);

        Assert.True(result);
        var deletedSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == SettingKeys.BotPrefix);
        Assert.Null(deletedSetting);
    }

    [Fact]
    public async Task DeleteSettingAsync_WithNonExistentKey_ReturnsFalse()
    {
        var result = await _service.DeleteSettingAsync(SettingKeys.BotPrefix);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteSettingAsync_WithNullKey_ReturnsFalse()
    {
        var result = await _service.DeleteSettingAsync(null!);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteSettingAsync_WithEmptyKey_ReturnsFalse()
    {
        var result = await _service.DeleteSettingAsync("");

        Assert.False(result);
    }

    [Fact]
    public async Task SettingExistsAsync_WithExistingKey_ReturnsTrue()
    {
        var setting = new Setting { Key = SettingKeys.BotPrefix, Value = "!" };
        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();

        var result = await _service.SettingExistsAsync(SettingKeys.BotPrefix);

        Assert.True(result);
    }

    [Fact]
    public async Task SettingExistsAsync_WithNonExistentKey_ReturnsFalse()
    {
        var result = await _service.SettingExistsAsync(SettingKeys.BotPrefix);

        Assert.False(result);
    }

    [Fact]
    public async Task SettingExistsAsync_WithNullKey_ReturnsFalse()
    {
        var result = await _service.SettingExistsAsync(null!);

        Assert.False(result);
    }

    [Fact]
    public async Task SettingExistsAsync_WithEmptyKey_ReturnsFalse()
    {
        var result = await _service.SettingExistsAsync("");

        Assert.False(result);
    }
}
