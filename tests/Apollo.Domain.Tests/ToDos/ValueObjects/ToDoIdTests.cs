using Apollo.Domain.ToDos.ValueObjects;

namespace Apollo.Domain.Tests.ToDos.ValueObjects;

public class ToDoIdTests
{
  [Fact]
  public void ToDoIdEqualityWorksCorrectly()
  {
    // Arrange
    var guid = Guid.NewGuid();
    var toDoId1 = new ToDoId(guid);
    var toDoId2 = new ToDoId(guid);
    var toDoId3 = new ToDoId(Guid.NewGuid());

    // Act & Assert
    Assert.Equal(toDoId1, toDoId2);
    Assert.NotEqual(toDoId1, toDoId3);
  }

  [Fact]
  public void ToDoIdValueIsAccessible()
  {
    // Arrange
    var guid = Guid.NewGuid();

    // Act
    var toDoId = new ToDoId(guid);

    // Assert
    Assert.Equal(guid, toDoId.Value);
  }
}
