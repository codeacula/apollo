using Apollo.Domain.ToDos.ValueObjects;

namespace Apollo.Domain.Tests.ToDos.ValueObjects;

public class AcknowledgedOnTests
{
  [Fact]
  public void AcknowledgedOnStoresValue()
  {
    // Arrange
    var date = DateTime.UtcNow;

    // Act
    var acknowledgedOn = new AcknowledgedOn(date);

    // Assert
    Assert.Equal(date, acknowledgedOn.Value);
  }

  [Fact]
  public void AcknowledgedOnEqualityWorksCorrectly()
  {
    // Arrange
    var date = DateTime.UtcNow;
    var acknowledgedOn1 = new AcknowledgedOn(date);
    var acknowledgedOn2 = new AcknowledgedOn(date);
    var acknowledgedOn3 = new AcknowledgedOn(date.AddMinutes(5));

    // Act & Assert
    Assert.Equal(acknowledgedOn1, acknowledgedOn2);
    Assert.NotEqual(acknowledgedOn1, acknowledgedOn3);
  }
}
