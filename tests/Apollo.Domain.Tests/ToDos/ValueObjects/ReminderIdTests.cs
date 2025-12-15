using Apollo.Domain.ToDos.ValueObjects;

namespace Apollo.Domain.Tests.ToDos.ValueObjects;

public class ReminderIdTests
{
  [Fact]
  public void ReminderIdEqualityWorksCorrectly()
  {
    // Arrange
    var guid = Guid.NewGuid();
    var reminderId1 = new ReminderId(guid);
    var reminderId2 = new ReminderId(guid);
    var reminderId3 = new ReminderId(Guid.NewGuid());

    // Act & Assert
    Assert.Equal(reminderId1, reminderId2);
    Assert.NotEqual(reminderId1, reminderId3);
  }

  [Fact]
  public void ReminderIdValueIsAccessible()
  {
    // Arrange
    var guid = Guid.NewGuid();

    // Act
    var reminderId = new ReminderId(guid);

    // Assert
    Assert.Equal(guid, reminderId.Value);
  }
}
