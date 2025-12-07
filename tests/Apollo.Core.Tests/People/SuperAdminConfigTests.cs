using Apollo.Core.People;

namespace Apollo.Core.Tests.People;

public class SuperAdminConfigTests
{
  [Fact]
  public void SuperAdminConfig_CanBeInitialized()
  {
    // Arrange & Act
    var config = new SuperAdminConfig
    {
      DiscordUsername = "admin"
    };

    // Assert
    Assert.Equal("admin", config.DiscordUsername);
  }

  [Fact]
  public void SuperAdminConfig_AllowsNullUsername()
  {
    // Arrange & Act
    var config = new SuperAdminConfig
    {
      DiscordUsername = null
    };

    // Assert
    Assert.Null(config.DiscordUsername);
  }

  [Fact]
  public void SuperAdminConfig_DefaultsToNull()
  {
    // Arrange & Act
    var config = new SuperAdminConfig();

    // Assert
    Assert.Null(config.DiscordUsername);
  }
}
