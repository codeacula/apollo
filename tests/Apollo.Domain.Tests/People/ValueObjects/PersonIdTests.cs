using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Tests.People.ValueObjects;

public class PersonIdTests
{
  [Fact]
  public void ImplicitCastToGuidReturnsValue()
  {
    // Arrange
    var guid = Guid.NewGuid();

    // Act
    Guid result = new PersonId(guid);

    // Assert
    Assert.Equal(guid, result);
  }

  [Fact]
  public void PersonIdEqualityWorksCorrectly()
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
  public void PersonIdValueIsAccessible()
  {
    // Arrange
    var guid = Guid.NewGuid();

    // Act
    var personId = new PersonId(guid);

    // Assert
    Assert.Equal(guid, personId.Value);
  }
}
