using Apollo.Domain.ToDos.ValueObjects;

namespace Apollo.Domain.Tests.ToDos.ValueObjects;

public class DescriptionTests
{
  [Fact]
  public void DescriptionStoresValue()
  {
    // Arrange & Act
    var description = new Description("Buy groceries");

    // Assert
    Assert.Equal("Buy groceries", description.Value);
  }

  [Fact]
  public void DescriptionEqualityWorksCorrectly()
  {
    // Arrange
    var description1 = new Description("Task");
    var description2 = new Description("Task");
    var description3 = new Description("Different");

    // Act & Assert
    Assert.Equal(description1, description2);
    Assert.NotEqual(description1, description3);
  }
}
