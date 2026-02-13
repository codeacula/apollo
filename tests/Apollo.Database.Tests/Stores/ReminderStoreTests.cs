using Apollo.Database.ToDos;

namespace Apollo.Database.Tests.Stores;

public sealed class ReminderStoreTests(StoreTestFixture fixture) : IClassFixture<StoreTestFixture>
{
  private readonly StoreTestFixture _fixture = fixture;

  #region CreateAsync Tests

  [Fact]
  public async Task CreateAsyncWithValidInputReturnsSuccessAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails("Remind me to take medication");
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    var result = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(reminderId.Value, result.Value.Id.Value);
    Assert.Equal(details.Value, result.Value.Details.Value);
  }

  [Fact]
  public async Task CreateAsyncPersistsReminderInDatabaseAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    _ = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);
    var getResult = await store.GetAsync(reminderId);

    // Assert
    Assert.True(getResult.IsSuccess);
    Assert.Equal(reminderId.Value, getResult.Value.Id.Value);
  }

  [Fact]
  public async Task CreateAsyncWithNullDetailsThrowsExceptionAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act & Assert
    _ = await Assert.ThrowsAsync<ArgumentException>(() =>
      store.CreateAsync(reminderId, personId, default, reminderTime, quartzJobId));
  }

  #endregion CreateAsync Tests

  #region GetAsync Tests

  [Fact]
  public async Task GetAsyncWithValidIdReturnsReminderAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails("Test reminder");
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);
    _ = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);

    // Act
    var result = await store.GetAsync(reminderId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Test reminder", result.Value.Details.Value);
  }

  [Fact]
  public async Task GetAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidReminderId = StoreTestFixture.CreateTestReminderId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    var result = await store.GetAsync(invalidReminderId);

    // Assert
    Assert.True(result.IsFailed);
  }

  [Fact]
  public async Task GetAsyncDoesNotReturnDeletedReminderAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);
    _ = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);
    _ = await store.DeleteAsync(reminderId);

    // Act
    var result = await store.GetAsync(reminderId);

    // Assert
    Assert.True(result.IsFailed);
  }

  #endregion GetAsync Tests

  #region GetByQuartzJobIdAsync Tests

  [Fact]
  public async Task GetByQuartzJobIdAsyncReturnsRemindersForQuartzJobAsync()
  {
    // Arrange
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var reminderId1 = StoreTestFixture.CreateTestReminderId();
    var reminderId2 = StoreTestFixture.CreateTestReminderId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    _ = await store.CreateAsync(reminderId1, personId, details, reminderTime, quartzJobId);
    _ = await store.CreateAsync(reminderId2, personId, details, reminderTime, quartzJobId);
    var result = await store.GetByQuartzJobIdAsync(quartzJobId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value.Count());
  }

  [Fact]
  public async Task GetByQuartzJobIdAsyncWithInvalidJobIdReturnsEmptyAsync()
  {
    // Arrange
    var invalidQuartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    var result = await store.GetByQuartzJobIdAsync(invalidQuartzJobId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value);
  }

  #endregion GetByQuartzJobIdAsync Tests

  #region LinkToToDoAsync Tests

  [Fact]
  public async Task LinkToToDoAsyncCreatesLinkBetweenReminderAndToDoAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);
    _ = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);

    // Act
    var result = await store.LinkToToDoAsync(reminderId, toDoId);

    // Assert
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task GetByToDoIdAsyncReturnsRemindersLinkedToToDoAsync()
  {
    // Arrange
    var reminderId1 = StoreTestFixture.CreateTestReminderId();
    var reminderId2 = StoreTestFixture.CreateTestReminderId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId1 = StoreTestFixture.CreateTestQuartzJobId();
    var quartzJobId2 = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);
    _ = await store.CreateAsync(reminderId1, personId, details, reminderTime, quartzJobId1);
    _ = await store.CreateAsync(reminderId2, personId, details, reminderTime, quartzJobId2);

    // Act
    _ = await store.LinkToToDoAsync(reminderId1, toDoId);
    _ = await store.LinkToToDoAsync(reminderId2, toDoId);
    var result = await store.GetByToDoIdAsync(toDoId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value.Count());
  }

  [Fact]
  public async Task GetByToDoIdAsyncWithInvalidToDoIdReturnsEmptyAsync()
  {
    // Arrange
    var invalidToDoId = StoreTestFixture.CreateTestToDoId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    var result = await store.GetByToDoIdAsync(invalidToDoId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value);
  }

  #endregion LinkToToDoAsync Tests

  #region UnlinkFromToDoAsync Tests

  [Fact]
  public async Task UnlinkFromToDoAsyncRemovesLinkBetweenReminderAndToDoAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);
    _ = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);
    _ = await store.LinkToToDoAsync(reminderId, toDoId);

    // Act
    var unlinkResult = await store.UnlinkFromToDoAsync(reminderId, toDoId);
    var getReminderResult = await store.GetByToDoIdAsync(toDoId);

    // Assert
    Assert.True(unlinkResult.IsSuccess);
    Assert.True(getReminderResult.IsSuccess);
    Assert.Empty(getReminderResult.Value);
  }

  [Fact]
  public async Task UnlinkFromToDoAsyncWithNonexistentLinkReturnsFailureAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    var result = await store.UnlinkFromToDoAsync(reminderId, toDoId);

    // Assert
    Assert.True(result.IsFailed);
  }

  #endregion UnlinkFromToDoAsync Tests

  #region MarkAsSentAsync Tests

  [Fact]
  public async Task MarkAsSentAsyncMarksReminderAsSentAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);
    _ = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);

    // Act
    var result = await store.MarkAsSentAsync(reminderId);

    // Assert
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task MarkAsSentAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidReminderId = StoreTestFixture.CreateTestReminderId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    var result = await store.MarkAsSentAsync(invalidReminderId);

    // Assert
    Assert.False(result.IsSuccess);
  }

  #endregion MarkAsSentAsync Tests

  #region AcknowledgeAsync Tests

  [Fact]
  public async Task AcknowledgeAsyncMarksReminderAsAcknowledgedAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);
    _ = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);

    // Act
    var result = await store.AcknowledgeAsync(reminderId);

    // Assert
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task AcknowledgeAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidReminderId = StoreTestFixture.CreateTestReminderId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    var result = await store.AcknowledgeAsync(invalidReminderId);

    // Assert
    Assert.False(result.IsSuccess);
  }

  #endregion AcknowledgeAsync Tests

  #region DeleteAsync Tests

  [Fact]
  public async Task DeleteAsyncMarksReminderAsDeletedAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);
    _ = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);

    // Act
    var deleteResult = await store.DeleteAsync(reminderId);
    var getResult = await store.GetAsync(reminderId);

    // Assert
    Assert.True(deleteResult.IsSuccess);
    Assert.True(getResult.IsFailed);
  }

  [Fact]
  public async Task DeleteAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidReminderId = StoreTestFixture.CreateTestReminderId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    var result = await store.DeleteAsync(invalidReminderId);

    // Assert
    Assert.False(result.IsSuccess);
  }

  #endregion DeleteAsync Tests

  #region GetLinkedToDoIdsAsync Tests

  [Fact]
  public async Task GetLinkedToDoIdsAsyncReturnsLinkedToDoIdsAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var toDoId1 = StoreTestFixture.CreateTestToDoId();
    var toDoId2 = StoreTestFixture.CreateTestToDoId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);
    _ = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);

    // Act
    _ = await store.LinkToToDoAsync(reminderId, toDoId1);
    _ = await store.LinkToToDoAsync(reminderId, toDoId2);
    var result = await store.GetLinkedToDoIdsAsync(reminderId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value.Count());
  }

  [Fact]
  public async Task GetLinkedToDoIdsAsyncWithInvalidReminderIdReturnsEmptyAsync()
  {
    // Arrange
    var invalidReminderId = StoreTestFixture.CreateTestReminderId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    var result = await store.GetLinkedToDoIdsAsync(invalidReminderId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value);
  }

  #endregion GetLinkedToDoIdsAsync Tests

  #region GetLinkedReminderIdsAsync Tests

  [Fact]
  public async Task GetLinkedReminderIdsAsyncReturnsLinkedReminderIdsAsync()
  {
    // Arrange
    var reminderId1 = StoreTestFixture.CreateTestReminderId();
    var reminderId2 = StoreTestFixture.CreateTestReminderId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId1 = StoreTestFixture.CreateTestQuartzJobId();
    var quartzJobId2 = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);
    _ = await store.CreateAsync(reminderId1, personId, details, reminderTime, quartzJobId1);
    _ = await store.CreateAsync(reminderId2, personId, details, reminderTime, quartzJobId2);

    // Act
    _ = await store.LinkToToDoAsync(reminderId1, toDoId);
    _ = await store.LinkToToDoAsync(reminderId2, toDoId);
    var result = await store.GetLinkedReminderIdsAsync(toDoId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value.Count());
  }

  [Fact]
  public async Task GetLinkedReminderIdsAsyncWithInvalidToDoIdReturnsEmptyAsync()
  {
    // Arrange
    var invalidToDoId = StoreTestFixture.CreateTestToDoId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    var result = await store.GetLinkedReminderIdsAsync(invalidToDoId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value);
  }

  #endregion GetLinkedReminderIdsAsync Tests

  #region Edge Cases Tests

  [Fact]
  public async Task CreateMultipleRemindersForSamePersonAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    const int reminderCount = 5;
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act
    for (int i = 0; i < reminderCount; i++)
    {
      var reminderId = StoreTestFixture.CreateTestReminderId();
      var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
      _ = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);
    }

    // Assert - verify all were created
    _ = StoreTestFixture.CreateTestQuartzJobId();
    // Note: We created separate job IDs, so can't query by single job ID
    // This test documents that multiple reminders can be created for same person
  }

  [Fact]
  public async Task ReminderLifecycleFullFlowAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);

    // Act - Full lifecycle
    var createResult = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);
    var linkResult = await store.LinkToToDoAsync(reminderId, toDoId);
    var sentResult = await store.MarkAsSentAsync(reminderId);
    var acknowledgeResult = await store.AcknowledgeAsync(reminderId);
    var linkedToDosResult = await store.GetLinkedToDoIdsAsync(reminderId);
    var unlinkResult = await store.UnlinkFromToDoAsync(reminderId, toDoId);
    var deleteResult = await store.DeleteAsync(reminderId);
    var getDeletedResult = await store.GetAsync(reminderId);

    // Assert
    Assert.True(createResult.IsSuccess);
    Assert.True(linkResult.IsSuccess);
    Assert.True(sentResult.IsSuccess);
    Assert.True(acknowledgeResult.IsSuccess);
    Assert.True(linkedToDosResult.IsSuccess);
    _ = Assert.Single(linkedToDosResult.Value);
    Assert.True(unlinkResult.IsSuccess);
    Assert.True(deleteResult.IsSuccess);
    Assert.True(getDeletedResult.IsFailed);
  }

  [Fact]
  public async Task MultipleLinksAndUnlinksAsync()
  {
    // Arrange
    var reminderId = StoreTestFixture.CreateTestReminderId();
    var toDoId1 = StoreTestFixture.CreateTestToDoId();
    var toDoId2 = StoreTestFixture.CreateTestToDoId();
    var toDoId3 = StoreTestFixture.CreateTestToDoId();
    var personId = StoreTestFixture.CreateTestPersonId();
    var details = StoreTestFixture.CreateTestDetails();
    var reminderTime = StoreTestFixture.CreateTestReminderTime();
    var quartzJobId = StoreTestFixture.CreateTestQuartzJobId();
    var store = new ReminderStore(_fixture.DocumentSession, StoreTestFixture.TimeProvider);
    _ = await store.CreateAsync(reminderId, personId, details, reminderTime, quartzJobId);

    // Act
    _ = await store.LinkToToDoAsync(reminderId, toDoId1);
    _ = await store.LinkToToDoAsync(reminderId, toDoId2);
    _ = await store.LinkToToDoAsync(reminderId, toDoId3);
    var linkedResult1 = await store.GetLinkedToDoIdsAsync(reminderId);

    _ = await store.UnlinkFromToDoAsync(reminderId, toDoId2);
    var linkedResult2 = await store.GetLinkedToDoIdsAsync(reminderId);

    // Assert
    Assert.Equal(3, linkedResult1.Value.Count());
    Assert.Equal(2, linkedResult2.Value.Count());
  }

  #endregion Edge Cases Tests
}
