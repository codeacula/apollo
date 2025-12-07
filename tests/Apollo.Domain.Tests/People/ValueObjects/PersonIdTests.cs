using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Tests.People.ValueObjects;

public class PersonIdTests
{
  [Fact]
  public void ImplicitCastToGuid_ReturnsValue()
  {
    // Arrange
    var guid = Guid.NewGuid();
    var personId = new PersonId(guid);

    // Act
    Guid result = personId;

    // Assert
    Assert.Equal(guid, result);
  }

  [Fact]
  public void PersonId_Equality_WorksCorrectly()
  {
    // Arrange
    var guid = Guid.NewGuid();
    var personId1 = new PersonId(guid);
    var personId2 = new PersonId(guid);
    var personId3 = new PersonId(Guid.NewGuid());

    // Act & Assert
    Assert.Equal(personId1, personId2);
    Assert.NotEqual(personId1, personId3);
  }

  [Fact]
  public void PersonId_Value_IsAccessible()
  {
    // Arrange
    var guid = Guid.NewGuid();

    // Act
    var personId = new PersonId(guid);

    // Assert
    Assert.Equal(guid, personId.Value);
  }
}
