using Apollo.Core.People;

namespace Apollo.Core.Tests.People;

public class SuperAdminConfigTests
{
  [Fact]
  public void SuperAdminConfigCanBeInitialized()
  {
    // Arrange & Act
    var config = new SuperAdminConfig
    {
      DiscordUserId = "admin"
    };

    // Assert
    Assert.Equal("admin", config.DiscordUserId);
  }

  [Fact]
  public void SuperAdminConfigAllowsNullUsername()
  {
    // Arrange & Act
    var config = new SuperAdminConfig
    {
      DiscordUserId = null
    };

    // Assert
    Assert.Null(config.DiscordUserId);
  }

  [Fact]
  public void SuperAdminConfigDefaultsToNull()
  {
    // Arrange & Act
    var config = new SuperAdminConfig();

    // Assert
    Assert.Null(config.DiscordUserId);
  }
}
