using Apollo.Domain.Common.ValueObjects;

namespace Apollo.Domain.Tests.Common.ValueObjects;

public class UpdatedOnTests
{
  [Fact]
  public void UpdatedOnStoresValue()
  {
    // Arrange
    var date = DateTime.UtcNow;

    // Act
    var updatedOn = new UpdatedOn(date);

    // Assert
    Assert.Equal(date, updatedOn.Value);
  }

  [Fact]
  public void UpdatedOnEqualityWorksCorrectly()
  {
    // Arrange
    var date = DateTime.UtcNow;
    var updatedOn1 = new UpdatedOn(date);
    var updatedOn2 = new UpdatedOn(date);
    var updatedOn3 = new UpdatedOn(date.AddMinutes(1));

    // Act & Assert
    Assert.Equal(updatedOn1, updatedOn2);
    Assert.NotEqual(updatedOn1, updatedOn3);
  }
}
