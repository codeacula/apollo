using Apollo.Discord.Config;

namespace Apollo.Discord.Tests.Config;

public class DiscordConfigTests
{
  [Fact]
  public void DiscordConfigShouldHaveDefaultBotName()
  {
    // Arrange & Act
    var config = new DiscordConfig();

    // Assert
    Assert.Equal("Apollo", config.BotName);
  }

  [Fact]
  public void DiscordConfigShouldAllowCustomBotName()
  {
    // Arrange & Act
    var config = new DiscordConfig { BotName = "TestBot" };

    // Assert
    Assert.Equal("TestBot", config.BotName);
  }
}
