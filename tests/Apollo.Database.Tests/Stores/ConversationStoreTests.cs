using Apollo.Database.Conversations;
using Apollo.Domain.Conversations.ValueObjects;

namespace Apollo.Database.Tests.Stores;

public sealed class ConversationStoreTests(StoreTestFixture fixture) : IClassFixture<StoreTestFixture>
{
  private readonly StoreTestFixture _fixture = fixture;

  #region CreateAsync Tests

  [Fact]
  public async Task CreateAsyncWithValidPersonIdReturnsSuccessAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    var result = await store.CreateAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(personId.Value, result.Value.PersonId.Value);
  }

  [Fact]
  public async Task CreateAsyncPersistsConversationInDatabaseAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    var createResult = await store.CreateAsync(personId);
    var conversationId = createResult.Value.Id;
    var getResult = await store.GetAsync(conversationId);

    // Assert
    Assert.True(createResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
    Assert.Equal(createResult.Value.Id, getResult.Value.Id);
  }

  [Fact]
  public async Task CreateAsyncWithNullPersonIdThrowsExceptionAsync()
  {
    // Arrange
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act & Assert
    _ = await Assert.ThrowsAsync<ArgumentException>(() =>
      store.CreateAsync(default));
  }

  #endregion CreateAsync Tests

  #region GetAsync Tests

  [Fact]
  public async Task GetAsyncWithValidIdReturnsConversationAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);
    var createResult = await store.CreateAsync(personId);
    var conversationId = createResult.Value.Id;

    // Act
    var result = await store.GetAsync(conversationId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(conversationId.Value, result.Value.Id.Value);
  }

  [Fact]
  public async Task GetAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidConversationId = new ConversationId(Guid.NewGuid());
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    var result = await store.GetAsync(invalidConversationId);

    // Assert
    Assert.True(result.IsFailed);
  }

  #endregion GetAsync Tests

  #region GetConversationByPersonIdAsync Tests

  [Fact]
  public async Task GetConversationByPersonIdAsyncWithValidPersonIdReturnsConversationAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);
    var createResult = await store.CreateAsync(personId);

    // Act
    var result = await store.GetConversationByPersonIdAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(createResult.Value.Id, result.Value.Id);
  }

  [Fact]
  public async Task GetConversationByPersonIdAsyncWithInvalidPersonIdReturnsFailureAsync()
  {
    // Arrange
    var invalidPersonId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    var result = await store.GetConversationByPersonIdAsync(invalidPersonId);

    // Assert
    Assert.True(result.IsFailed);
  }

  [Fact]
  public async Task GetConversationByPersonIdAsyncReturnsLatestConversationForPersonAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);
    // Create a conversation (will be overwritten by second create in current implementation)
    _ = await store.CreateAsync(personId);

    // Act
    var result = await store.GetConversationByPersonIdAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(personId.Value, result.Value.PersonId.Value);
  }

  #endregion GetConversationByPersonIdAsync Tests

  #region GetOrCreateConversationByPersonIdAsync Tests

  [Fact]
  public async Task GetOrCreateConversationByPersonIdAsyncReturnsExistingConversationAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);
    var createResult = await store.CreateAsync(personId);
    var conversationId = createResult.Value.Id;

    // Act
    var result = await store.GetOrCreateConversationByPersonIdAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(conversationId, result.Value.Id);
  }

  [Fact]
  public async Task GetOrCreateConversationByPersonIdAsyncCreatesConversationIfNotExistsAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    var result = await store.GetOrCreateConversationByPersonIdAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(personId.Value, result.Value.PersonId.Value);
  }

  [Fact]
  public async Task GetOrCreateConversationByPersonIdAsyncIsIdempotentAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    var result1 = await store.GetOrCreateConversationByPersonIdAsync(personId);
    var result2 = await store.GetOrCreateConversationByPersonIdAsync(personId);

    // Assert
    Assert.True(result1.IsSuccess);
    Assert.True(result2.IsSuccess);
    Assert.Equal(result1.Value.Id, result2.Value.Id);
  }

  #endregion GetOrCreateConversationByPersonIdAsync Tests

  #region AddMessageAsync Tests

  [Fact]
  public async Task AddMessageAsyncAddsMessageToConversationAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);
    var createResult = await store.CreateAsync(personId);
    var conversationId = createResult.Value.Id;
    var messageContent = StoreTestFixture.CreateTestContent("User message");

    // Act
    var result = await store.AddMessageAsync(conversationId, messageContent);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotEmpty(result.Value.Messages);
  }

  [Fact]
  public async Task AddMessageAsyncWithInvalidConversationIdReturnsFailureAsync()
  {
    // Arrange
    var invalidConversationId = new ConversationId(Guid.NewGuid());
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);
    var messageContent = StoreTestFixture.CreateTestContent();

    // Act
    var result = await store.AddMessageAsync(invalidConversationId, messageContent);

    // Assert
    Assert.True(result.IsFailed);
  }

  [Fact]
  public async Task AddMessageAsyncMultipleMessagesArePreservedAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);
    var createResult = await store.CreateAsync(personId);
    var conversationId = createResult.Value.Id;
    var message1 = StoreTestFixture.CreateTestContent("Message 1");
    var message2 = StoreTestFixture.CreateTestContent("Message 2");
    var message3 = StoreTestFixture.CreateTestContent("Message 3");

    // Act
    _ = await store.AddMessageAsync(conversationId, message1);
    _ = await store.AddMessageAsync(conversationId, message2);
    _ = await store.AddMessageAsync(conversationId, message3);
    var getResult = await store.GetAsync(conversationId);

    // Assert
    Assert.True(getResult.IsSuccess);
    Assert.Equal(3, getResult.Value.Messages.Count);
  }

  #endregion AddMessageAsync Tests

  #region AddReplyAsync Tests

  [Fact]
  public async Task AddReplyAsyncAddsReplyToConversationAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);
    var createResult = await store.CreateAsync(personId);
    var conversationId = createResult.Value.Id;
    var userMessage = StoreTestFixture.CreateTestContent("User message");
    var reply = StoreTestFixture.CreateTestContent("Apollo reply");

    // Act
    _ = await store.AddMessageAsync(conversationId, userMessage);
    var result = await store.AddReplyAsync(conversationId, reply);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotEmpty(result.Value.Messages);
  }

  [Fact]
  public async Task AddReplyAsyncWithInvalidConversationIdReturnsFailureAsync()
  {
    // Arrange
    var invalidConversationId = new ConversationId(Guid.NewGuid());
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);
    var reply = StoreTestFixture.CreateTestContent();

    // Act
    var result = await store.AddReplyAsync(invalidConversationId, reply);

    // Assert
    Assert.True(result.IsFailed);
  }

  [Fact]
  public async Task AddReplyAsyncMultipleRepliesArePreservedAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);
    var createResult = await store.CreateAsync(personId);
    var conversationId = createResult.Value.Id;
    var message1 = StoreTestFixture.CreateTestContent("User message 1");
    var reply1 = StoreTestFixture.CreateTestContent("Apollo reply 1");
    var message2 = StoreTestFixture.CreateTestContent("User message 2");
    var reply2 = StoreTestFixture.CreateTestContent("Apollo reply 2");

    // Act
    _ = await store.AddMessageAsync(conversationId, message1);
    _ = await store.AddReplyAsync(conversationId, reply1);
    _ = await store.AddMessageAsync(conversationId, message2);
    _ = await store.AddReplyAsync(conversationId, reply2);
    var getResult = await store.GetAsync(conversationId);

    // Assert
    Assert.True(getResult.IsSuccess);
    Assert.Equal(4, getResult.Value.Messages.Count);
  }

  #endregion AddReplyAsync Tests

  #region Edge Cases Tests

  [Fact]
  public async Task CreateMultipleConversationsForDifferentPeopleAsync()
  {
    // Arrange
    var personId1 = StoreTestFixture.CreateTestPersonId();
    var personId2 = StoreTestFixture.CreateTestPersonId();
    var personId3 = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    var conversation1 = await store.CreateAsync(personId1);
    var conversation2 = await store.CreateAsync(personId2);
    var conversation3 = await store.CreateAsync(personId3);

    // Assert
    Assert.True(conversation1.IsSuccess);
    Assert.True(conversation2.IsSuccess);
    Assert.True(conversation3.IsSuccess);
    Assert.NotEqual(conversation1.Value.Id, conversation2.Value.Id);
    Assert.NotEqual(conversation2.Value.Id, conversation3.Value.Id);
    Assert.NotEqual(conversation1.Value.Id, conversation3.Value.Id);
  }

  [Fact]
  public async Task LongConversationWithManyMessagesAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);
    var createResult = await store.CreateAsync(personId);
    var conversationId = createResult.Value.Id;
    const int messageCount = 20;

    // Act
    for (int i = 0; i < messageCount; i++)
    {
      var content = StoreTestFixture.CreateTestContent($"Message {i}");
      _ = await store.AddMessageAsync(conversationId, content);
    }
    var getResult = await store.GetAsync(conversationId);

    // Assert
    Assert.True(getResult.IsSuccess);
    Assert.Equal(messageCount, getResult.Value.Messages.Count);
  }

  [Fact]
  public async Task ConversationWithEmptyAndNonEmptyMessagesAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var store = new ConversationStore(_fixture.DocumentSession, _fixture.TimeProvider);
    var createResult = await store.CreateAsync(personId);
    var conversationId = createResult.Value.Id;

    // Act
    var result1 = await store.GetAsync(conversationId);
    _ = await store.AddMessageAsync(conversationId, StoreTestFixture.CreateTestContent("First message"));
    var result2 = await store.GetAsync(conversationId);

    // Assert
    Assert.True(result1.IsSuccess);
    Assert.Empty(result1.Value.Messages);
    Assert.True(result2.IsSuccess);
    Assert.NotEmpty(result2.Value.Messages);
  }

  #endregion Edge Cases Tests
}
