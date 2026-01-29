using Apollo.Domain.Conversations.ValueObjects;

namespace Apollo.Domain.Tests.Conversations.ValueObjects;

public class FromUserTests
{
  [Fact]
  public void FromUserStoresValue()
  {
    // Arrange & Act
    var fromUser = new FromUser(true);

    // Assert
    Assert.True(fromUser.Value);
  }

  [Fact]
  public void FromUserEqualityWorksCorrectly()
  {
    // Arrange
    var fromUser1 = new FromUser(true);
    var fromUser2 = new FromUser(true);
    var fromUser3 = new FromUser(false);

    // Act & Assert
    Assert.Equal(fromUser1, fromUser2);
    Assert.NotEqual(fromUser1, fromUser3);
  }
}
