using Apollo.Domain.Common.Enums;
using Apollo.Domain.ToDos.ValueObjects;

namespace Apollo.Domain.Tests.ToDos.ValueObjects;

public class PriorityTests
{
  [Fact]
  public void PriorityStoresValue()
  {
    // Arrange & Act
    var priority = new Priority(Level.Red);

    // Assert
    Assert.Equal(Level.Red, priority.Value);
  }

  [Fact]
  public void PriorityEqualityWorksCorrectly()
  {
    // Arrange
    var priority1 = new Priority(Level.Yellow);
    var priority2 = new Priority(Level.Yellow);
    var priority3 = new Priority(Level.Blue);

    // Act & Assert
    Assert.Equal(priority1, priority2);
    Assert.NotEqual(priority1, priority3);
  }
}
