using Apollo.Domain.Conversations.ValueObjects;

namespace Apollo.Domain.Tests.Conversations.ValueObjects;

public class MessageIdTests
{
  [Fact]
  public void MessageIdEqualityWorksCorrectly()
  {
    // Arrange
    var guid = Guid.NewGuid();
    var messageId1 = new MessageId(guid);
    var messageId2 = new MessageId(guid);
    var messageId3 = new MessageId(Guid.NewGuid());

    // Act & Assert
    Assert.Equal(messageId1, messageId2);
    Assert.NotEqual(messageId1, messageId3);
  }

  [Fact]
  public void MessageIdValueIsAccessible()
  {
    // Arrange
    var guid = Guid.NewGuid();

    // Act
    var messageId = new MessageId(guid);

    // Assert
    Assert.Equal(guid, messageId.Value);
  }
}
