using Apollo.Domain.Configuration.ValueObjects;

namespace Apollo.Domain.Tests.Configuration.ValueObjects;

public class ConfigurationKeyTests
{
  [Fact]
  public void ConfigurationKeyStoresValue()
  {
    // Arrange & Act
    var key = new ConfigurationKey("apollo_main");

    // Assert
    Assert.Equal("apollo_main", key.Value);
  }

  [Fact]
  public void ConfigurationKeyEqualityWorksCorrectly()
  {
    // Arrange
    var key1 = new ConfigurationKey("apollo_main");
    var key2 = new ConfigurationKey("apollo_main");
    var key3 = new ConfigurationKey("other_key");

    // Act & Assert
    Assert.Equal(key1, key2);
    Assert.NotEqual(key1, key3);
  }
}
