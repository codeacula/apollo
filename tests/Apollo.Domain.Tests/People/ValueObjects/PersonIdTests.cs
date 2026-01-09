using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Tests.People.ValueObjects;

public class PersonIdTests
{
  [Fact]
  public void PersonIdEqualityWorksCorrectly()
  {
    // Arrange
    var personId1 = new PersonId(Platform.Discord, "123");
    var personId2 = new PersonId(Platform.Discord, "123");
    var personId3 = new PersonId(Platform.Discord, "456");

    // Act & Assert
    Assert.Equal(personId1, personId2);
    Assert.NotEqual(personId1, personId3);
  }

  [Fact]
  public void PersonIdValueIncludesPlatform()
  {
    // Arrange
    var personId = new PersonId(Platform.Discord, "123");

    // Act
    var value = personId.Value;

    // Assert
    Assert.Equal("Discord:123", value);
  }

  [Fact]
  public void PersonIdParseRoundTripWorks()
  {
    // Arrange
    var personId = new PersonId(Platform.Discord, "123");

    // Act
    var parsed = PersonId.Parse(personId.Value);

    // Assert
    Assert.Equal(personId, parsed);
  }
}
