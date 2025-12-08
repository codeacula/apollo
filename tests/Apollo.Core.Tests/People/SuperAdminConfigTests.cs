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
      DiscordUsername = "admin"
    };

    // Assert
    Assert.Equal("admin", config.DiscordUsername);
  }

  [Fact]
  public void SuperAdminConfigAllowsNullUsername()
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
  public void SuperAdminConfigDefaultsToNull()
  {
    // Arrange & Act
    var config = new SuperAdminConfig();

    // Assert
    Assert.Null(config.DiscordUsername);
  }
}
