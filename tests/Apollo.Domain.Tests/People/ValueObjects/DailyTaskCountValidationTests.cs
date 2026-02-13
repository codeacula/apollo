using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Tests.People.ValueObjects;

public class DailyTaskCountValidationTests
{
  [Fact]
  public void ConstructorWithValueAbove20ThrowsArgumentOutOfRangeException()
  {
    // Act & Assert
    _ = Assert.Throws<ArgumentOutOfRangeException>(() => new DailyTaskCount(21));
  }

  [Fact]
  public void ConstructorWithValueBelow1ThrowsArgumentOutOfRangeException()
  {
    // Act & Assert
    _ = Assert.Throws<ArgumentOutOfRangeException>(() => new DailyTaskCount(0));
  }

  [Fact]
  public void ConstructorWithValidValueSetsValue()
  {
    // Act
    var result = new DailyTaskCount(10);

    // Assert
    Assert.Equal(10, result.Value);
  }
}
