using Apollo.Database.ToDos;
using Apollo.Domain.Common.Enums;

namespace Apollo.Database.Tests.Stores;

public sealed class ToDoStoreTests(StoreTestFixture fixture) : IClassFixture<StoreTestFixture>
{
  private readonly StoreTestFixture _fixture = fixture;

  #region CreateAsync Tests

  [Fact]
  public async Task CreateAsyncWithValidInputReturnsSuccessAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription("Buy groceries");
    var priority = StoreTestFixture.CreateTestPriority(Level.Red);
    var energy = StoreTestFixture.CreateTestEnergy(Level.Green);
    var interest = StoreTestFixture.CreateTestInterest(Level.Yellow);
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    var result = await store.CreateAsync(toDoId, personId, description, priority, energy, interest);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(toDoId.Value, result.Value.Id.Value);
    Assert.Equal(description.Value, result.Value.Description.Value);
  }

  [Fact]
  public async Task CreateAsyncPersistsToDoInDatabaseAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    _ = await store.CreateAsync(toDoId, personId, description, priority, energy, interest);
    var getResult = await store.GetAsync(toDoId);

    // Assert
    Assert.True(getResult.IsSuccess);
    Assert.Equal(toDoId.Value, getResult.Value.Id.Value);
  }

  [Fact]
  public async Task CreateAsyncWithNullDescriptionThrowsExceptionAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act & Assert
    _ = await Assert.ThrowsAsync<ArgumentException>(() =>
      store.CreateAsync(toDoId, personId, default, default, default, default));
  }

  #endregion CreateAsync Tests

  #region GetAsync Tests

  [Fact]
  public async Task GetAsyncWithValidIdReturnsToDoAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription("Complete project");
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId, personId, description, priority, energy, interest);

    // Act
    var result = await store.GetAsync(toDoId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Complete project", result.Value.Description.Value);
  }

  [Fact]
  public async Task GetAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidToDoId = StoreTestFixture.CreateTestToDoId();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    var result = await store.GetAsync(invalidToDoId);

    // Assert
    Assert.True(result.IsFailed);
  }

  [Fact]
  public async Task GetAsyncDoesNotReturnDeletedToDoAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId, personId, description, priority, energy, interest);
    _ = await store.DeleteAsync(toDoId);

    // Act
    var result = await store.GetAsync(toDoId);

    // Assert
    Assert.True(result.IsFailed);
  }

  #endregion GetAsync Tests

  #region GetByPersonIdAsync Tests

  [Fact]
  public async Task GetByPersonIdAsyncWithValidPersonIdReturnsAllActiveToDoesAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId1 = StoreTestFixture.CreateTestToDoId();
    var toDoId2 = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId1, personId, description, priority, energy, interest);
    _ = await store.CreateAsync(toDoId2, personId, description, priority, energy, interest);

    // Act
    var result = await store.GetByPersonIdAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value.Count());
  }

  [Fact]
  public async Task GetByPersonIdAsyncExcludesCompletedToDoesWhenIncludeCompletedIsFalseAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId1 = StoreTestFixture.CreateTestToDoId();
    var toDoId2 = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId1, personId, description, priority, energy, interest);
    _ = await store.CreateAsync(toDoId2, personId, description, priority, energy, interest);
    _ = await store.CompleteAsync(toDoId1);

    // Act
    var result = await store.GetByPersonIdAsync(personId, includeCompleted: false);

    // Assert
    Assert.True(result.IsSuccess);
    _ = Assert.Single(result.Value);
  }

  [Fact]
  public async Task GetByPersonIdAsyncIncludesCompletedToDoesWhenIncludeCompletedIsTrueAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId1 = StoreTestFixture.CreateTestToDoId();
    var toDoId2 = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId1, personId, description, priority, energy, interest);
    _ = await store.CreateAsync(toDoId2, personId, description, priority, energy, interest);
    _ = await store.CompleteAsync(toDoId1);

    // Act
    var result = await store.GetByPersonIdAsync(personId, includeCompleted: true);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value.Count());
  }

  [Fact]
  public async Task GetByPersonIdAsyncExcludesDeletedToDoesAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId1 = StoreTestFixture.CreateTestToDoId();
    var toDoId2 = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId1, personId, description, priority, energy, interest);
    _ = await store.CreateAsync(toDoId2, personId, description, priority, energy, interest);
    _ = await store.DeleteAsync(toDoId1);

    // Act
    var result = await store.GetByPersonIdAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    _ = Assert.Single(result.Value);
  }

  #endregion GetByPersonIdAsync Tests

  #region UpdateAsync Tests

  [Fact]
  public async Task UpdateAsyncWithValidInputUpdatesDescriptionAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var oldDescription = StoreTestFixture.CreateTestDescription("Original");
    var newDescription = StoreTestFixture.CreateTestDescription("Updated");
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId, personId, oldDescription, priority, energy, interest);

    // Act
    var updateResult = await store.UpdateAsync(toDoId, newDescription);
    var getResult = await store.GetAsync(toDoId);

    // Assert
    Assert.True(updateResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
    Assert.Equal("Updated", getResult.Value.Description.Value);
  }

  [Fact]
  public async Task UpdateAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidToDoId = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    var result = await store.UpdateAsync(invalidToDoId, description);

    // Assert
    Assert.False(result.IsSuccess);
  }

  #endregion UpdateAsync Tests

  #region CompleteAsync Tests

  [Fact]
  public async Task CompleteAsyncMarksToDoAsCompletedAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId, personId, description, priority, energy, interest);

    // Act
    var completeResult = await store.CompleteAsync(toDoId);
    // Note: After completion, the ToDo still exists in the database (marked as completed)
    // We verify the completion succeeded

    // Assert
    Assert.True(completeResult.IsSuccess);
  }

  [Fact]
  public async Task CompleteAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidToDoId = StoreTestFixture.CreateTestToDoId();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    var result = await store.CompleteAsync(invalidToDoId);

    // Assert
    Assert.False(result.IsSuccess);
  }

  #endregion CompleteAsync Tests

  #region DeleteAsync Tests

  [Fact]
  public async Task DeleteAsyncMarksToDoAsDeletedAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId, personId, description, priority, energy, interest);

    // Act
    var deleteResult = await store.DeleteAsync(toDoId);
    var getResult = await store.GetAsync(toDoId);

    // Assert
    Assert.True(deleteResult.IsSuccess);
    Assert.True(getResult.IsFailed);
  }

  [Fact]
  public async Task DeleteAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidToDoId = StoreTestFixture.CreateTestToDoId();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    var result = await store.DeleteAsync(invalidToDoId);

    // Assert
    Assert.False(result.IsSuccess);
  }

  #endregion DeleteAsync Tests

  #region Priority Tests

  [Fact]
  public async Task UpdatePriorityAsyncUpdatesPriorityAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority(Level.Green);
    var newPriority = StoreTestFixture.CreateTestPriority(Level.Red);
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId, personId, description, priority, energy, interest);

    // Act
    var updateResult = await store.UpdatePriorityAsync(toDoId, newPriority);
    var getResult = await store.GetAsync(toDoId);

    // Assert
    Assert.True(updateResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
    Assert.Equal(Level.Red, getResult.Value.Priority.Value);
  }

  #endregion Priority Tests

  #region Energy Tests

  [Fact]
  public async Task UpdateEnergyAsyncUpdatesEnergyAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy(Level.Blue);
    var newEnergy = StoreTestFixture.CreateTestEnergy(Level.Red);
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId, personId, description, priority, energy, interest);

    // Act
    var updateResult = await store.UpdateEnergyAsync(toDoId, newEnergy);
    var getResult = await store.GetAsync(toDoId);

    // Assert
    Assert.True(updateResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
    Assert.Equal(Level.Red, getResult.Value.Energy.Value);
  }

  #endregion Energy Tests

  #region Interest Tests

  [Fact]
  public async Task UpdateInterestAsyncUpdatesInterestAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest(Level.Blue);
    var newInterest = StoreTestFixture.CreateTestInterest(Level.Yellow);
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId, personId, description, priority, energy, interest);

    // Act
    var updateResult = await store.UpdateInterestAsync(toDoId, newInterest);
    var getResult = await store.GetAsync(toDoId);

    // Assert
    Assert.True(updateResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
    Assert.Equal(Level.Yellow, getResult.Value.Interest.Value);
  }

  #endregion Interest Tests

  #region Edge Cases Tests

  [Fact]
  public async Task CreateAsyncWithMultipleToDoesForSamePersonAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId1 = StoreTestFixture.CreateTestToDoId();
    var toDoId2 = StoreTestFixture.CreateTestToDoId();
    var toDoId3 = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);

    // Act
    _ = await store.CreateAsync(toDoId1, personId, description, priority, energy, interest);
    _ = await store.CreateAsync(toDoId2, personId, description, priority, energy, interest);
    _ = await store.CreateAsync(toDoId3, personId, description, priority, energy, interest);
    var result = await store.GetByPersonIdAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(3, result.Value.Count());
  }

  [Fact]
  public async Task CompleteAndDeleteToDoSequentiallyAsync()
  {
    // Arrange
    var personId = StoreTestFixture.CreateTestPersonId();
    var toDoId = StoreTestFixture.CreateTestToDoId();
    var description = StoreTestFixture.CreateTestDescription();
    var priority = StoreTestFixture.CreateTestPriority();
    var energy = StoreTestFixture.CreateTestEnergy();
    var interest = StoreTestFixture.CreateTestInterest();
    var store = new ToDoStore(_fixture.DocumentSession, _fixture.TimeProvider);
    _ = await store.CreateAsync(toDoId, personId, description, priority, energy, interest);

    // Act
    var completeResult = await store.CompleteAsync(toDoId);
    _ = await store.DeleteAsync(toDoId);
    var deletedResult = await store.GetAsync(toDoId);

    // Assert
    Assert.True(completeResult.IsSuccess);
    Assert.True(deletedResult.IsFailed);
  }

  #endregion Edge Cases Tests
}
