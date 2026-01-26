using Apollo.Domain.ToDos.ValueObjects;

namespace Apollo.Domain.Tests.ToDos.ValueObjects;

public class DueDateTests
{
  [Fact]
  public void DueDateStoresValue()
  {
    // Arrange
    var date = DateTime.UtcNow.AddDays(7);

    // Act
    var dueDate = new DueDate(date);

    // Assert
    Assert.Equal(date, dueDate.Value);
  }

  [Fact]
  public void DueDateEqualityWorksCorrectly()
  {
    // Arrange
    var date = DateTime.UtcNow.AddDays(3);
    var dueDate1 = new DueDate(date);
    var dueDate2 = new DueDate(date);
    var dueDate3 = new DueDate(date.AddDays(1));

    // Act & Assert
    Assert.Equal(dueDate1, dueDate2);
    Assert.NotEqual(dueDate1, dueDate3);
  }
}
