using Apollo.Domain.Conversations.ValueObjects;

namespace Apollo.Domain.Tests.Conversations.ValueObjects;

public class ConversationIdTests
{
  [Fact]
  public void ConversationIdEqualityWorksCorrectly()
  {
    // Arrange
    var guid = Guid.NewGuid();
    var conversationId1 = new ConversationId(guid);
    var conversationId2 = new ConversationId(guid);
    var conversationId3 = new ConversationId(Guid.NewGuid());

    // Act & Assert
    Assert.Equal(conversationId1, conversationId2);
    Assert.NotEqual(conversationId1, conversationId3);
  }

  [Fact]
  public void ConversationIdValueIsAccessible()
  {
    // Arrange
    var guid = Guid.NewGuid();

    // Act
    var conversationId = new ConversationId(guid);

    // Assert
    Assert.Equal(guid, conversationId.Value);
  }
}
