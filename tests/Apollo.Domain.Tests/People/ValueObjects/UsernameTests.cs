using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Tests.People.ValueObjects;

public class UsernameTests
{
  [Fact]
  public void IsValidWithNonEmptyValueReturnsTrue()
  {
    // Arrange
    var username = new Username("testuser");

    // Act & Assert
    Assert.True(username.IsValid);
  }

  [Fact]
  public void IsValidWithEmptyValueReturnsFalse()
  {
    // Arrange
    var username = new Username(string.Empty);

    // Act & Assert
    Assert.False(username.IsValid);
  }

  [Fact]
  public void IsValidWithWhitespaceValueReturnsFalse()
  {
    // Arrange
    var username = new Username("   ");

    // Act & Assert
    Assert.False(username.IsValid);
  }

  [Fact]
  public void ImplicitCastToStringReturnsValue()
  {
    // Act
    string value = new Username("testuser");

    // Assert
    Assert.Equal("testuser", value);
  }

  [Fact]
  public void UsernameEqualityWorksCorrectly()
  {
    // Arrange
    var username1 = new Username("testuser");
    var username2 = new Username("testuser");
    var username3 = new Username("otheruser");

    // Act & Assert
    Assert.Equal(username1, username2);
    Assert.NotEqual(username1, username3);
  }
}
