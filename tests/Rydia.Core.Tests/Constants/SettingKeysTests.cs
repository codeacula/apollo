using Rydia.Core.Constants;

namespace Rydia.Core.Tests.Constants;

public class SettingKeysTests
{
    [Fact]
    public void DailyAlertChannelId_ShouldHaveCorrectValue()
    {
        Assert.Equal("daily_alert_channel_id", SettingKeys.DailyAlertChannelId);
    }

    [Fact]
    public void DailyAlertRoleId_ShouldHaveCorrectValue()
    {
        Assert.Equal("daily_alert_role_id", SettingKeys.DailyAlertRoleId);
    }

    [Fact]
    public void DefaultTimezone_ShouldHaveCorrectValue()
    {
        Assert.Equal("default_timezone", SettingKeys.DefaultTimezone);
    }

    [Fact]
    public void BotPrefix_ShouldHaveCorrectValue()
    {
        Assert.Equal("bot_prefix", SettingKeys.BotPrefix);
    }

    [Fact]
    public void DebugLoggingEnabled_ShouldHaveCorrectValue()
    {
        Assert.Equal("debug_logging_enabled", SettingKeys.DebugLoggingEnabled);
    }

    [Fact]
    public void AllKeys_ShouldContainAllDefinedKeys()
    {
        var allKeys = SettingKeys.AllKeys;

        Assert.Contains(SettingKeys.DailyAlertChannelId, allKeys);
        Assert.Contains(SettingKeys.DailyAlertRoleId, allKeys);
        Assert.Contains(SettingKeys.DefaultTimezone, allKeys);
        Assert.Contains(SettingKeys.BotPrefix, allKeys);
        Assert.Contains(SettingKeys.DebugLoggingEnabled, allKeys);
    }

    [Fact]
    public void AllKeys_ShouldHaveCorrectCount()
    {
        Assert.Equal(5, SettingKeys.AllKeys.Count);
    }

    [Theory]
    [InlineData("daily_alert_channel_id", true)]
    [InlineData("daily_alert_role_id", true)]
    [InlineData("default_timezone", true)]
    [InlineData("bot_prefix", true)]
    [InlineData("debug_logging_enabled", true)]
    [InlineData("invalid_key", false)]
    [InlineData("", false)]
    [InlineData("random_key", false)]
    public void IsValidKey_ShouldReturnCorrectResult(string key, bool expectedResult)
    {
        var result = SettingKeys.IsValidKey(key);
        Assert.Equal(expectedResult, result);
    }
}
