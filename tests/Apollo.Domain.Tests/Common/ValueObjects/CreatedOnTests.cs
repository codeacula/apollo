using Apollo.Domain.Common.ValueObjects;

namespace Apollo.Domain.Tests.Common.ValueObjects;

public class CreatedOnTests
{
  [Fact]
  public void CreatedOnStoresValue()
  {
    // Arrange
    var date = DateTime.UtcNow;

    // Act
    var createdOn = new CreatedOn(date);

    // Assert
    Assert.Equal(date, createdOn.Value);
  }

  [Fact]
  public void CreatedOnEqualityWorksCorrectly()
  {
    // Arrange
    var date = DateTime.UtcNow;
    var createdOn1 = new CreatedOn(date);
    var createdOn2 = new CreatedOn(date);
    var createdOn3 = new CreatedOn(date.AddMinutes(1));

    // Act & Assert
    Assert.Equal(createdOn1, createdOn2);
    Assert.NotEqual(createdOn1, createdOn3);
  }
}
