using Apollo.Domain.Common.ValueObjects;

namespace Apollo.Domain.Tests.Common.ValueObjects;

public class DisplayNameTests
{
  [Fact]
  public void DisplayNameStoresValue()
  {
    // Arrange & Act
    var displayName = new DisplayName("Test Display Name");

    // Assert
    Assert.Equal("Test Display Name", displayName.Value);
  }

  [Fact]
  public void DisplayNameEqualityWorksCorrectly()
  {
    // Arrange
    var displayName1 = new DisplayName("Test");
    var displayName2 = new DisplayName("Test");
    var displayName3 = new DisplayName("Other");

    // Act & Assert
    Assert.Equal(displayName1, displayName2);
    Assert.NotEqual(displayName1, displayName3);
  }
}
