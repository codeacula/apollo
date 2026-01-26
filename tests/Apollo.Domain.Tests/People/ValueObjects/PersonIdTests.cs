using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Tests.People.ValueObjects;

public class PersonIdTests
{
  [Fact]
  public void PersonIdEqualityWorksCorrectly()
  {
    // Arrange
    var guid1 = Guid.NewGuid();
    var guid2 = Guid.NewGuid();
    var personId1 = new PersonId(guid1);
    var personId2 = new PersonId(guid1);
    var personId3 = new PersonId(guid2);

    // Act & Assert
    Assert.Equal(personId1, personId2);
    Assert.NotEqual(personId1, personId3);
  }
}
