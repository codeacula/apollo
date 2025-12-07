using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Tests.People.ValueObjects;

public class UsernameTests
{
  [Fact]
  public void IsValid_WithNonEmptyValue_ReturnsTrue()
  {
    // Arrange
    var username = new Username("testuser", Platform.Discord);

    // Act & Assert
    Assert.True(username.IsValid);
  }

  [Fact]
  public void IsValid_WithEmptyValue_ReturnsFalse()
  {
    // Arrange
    var username = new Username("", Platform.Discord);

    // Act & Assert
    Assert.False(username.IsValid);
  }

  [Fact]
  public void IsValid_WithWhitespaceValue_ReturnsFalse()
  {
    // Arrange
    var username = new Username("   ", Platform.Discord);

    // Act & Assert
    Assert.False(username.IsValid);
  }

  [Fact]
  public void ImplicitCastToString_ReturnsValue()
  {
    // Arrange
    var username = new Username("testuser", Platform.Discord);

    // Act
    string value = username;

    // Assert
    Assert.Equal("testuser", value);
  }

  [Fact]
  public void Username_WithPlatform_StoresPlatform()
  {
    // Arrange & Act
    var username = new Username("testuser", Platform.Discord);

    // Assert
    Assert.Equal(Platform.Discord, username.Platform);
  }

  [Fact]
  public void Username_Equality_WorksCorrectly()
  {
    // Arrange
    var username1 = new Username("testuser", Platform.Discord);
    var username2 = new Username("testuser", Platform.Discord);
    var username3 = new Username("otheruser", Platform.Discord);

    // Act & Assert
    Assert.Equal(username1, username2);
    Assert.NotEqual(username1, username3);
  }
}
