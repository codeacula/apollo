using Apollo.Domain.ToDos.ValueObjects;

namespace Apollo.Domain.Tests.ToDos.ValueObjects;

public class DetailsTests
{
  [Fact]
  public void DetailsStoresValue()
  {
    // Arrange & Act
    var details = new Details("Need to get milk, eggs, and bread");

    // Assert
    Assert.Equal("Need to get milk, eggs, and bread", details.Value);
  }

  [Fact]
  public void DetailsEqualityWorksCorrectly()
  {
    // Arrange
    var details1 = new Details("Some details");
    var details2 = new Details("Some details");
    var details3 = new Details("Other details");

    // Act & Assert
    Assert.Equal(details1, details2);
    Assert.NotEqual(details1, details3);
  }
}
