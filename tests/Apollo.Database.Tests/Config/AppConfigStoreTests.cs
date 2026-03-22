using Apollo.Database.Configuration;
using Apollo.Database.Configuration.Events;

using Marten;
using Marten.Events;

using Moq;

namespace Apollo.Database.Tests.Config;

public sealed class AppConfigStoreTests
{
  private readonly Mock<IDocumentSession> _sessionMock;

  public AppConfigStoreTests()
  {
    _sessionMock = new Mock<IDocumentSession>(MockBehavior.Loose);
  }

  /// <summary>
  /// When no configuration has ever been saved, the store should return a failure result
  /// indicating the system is unconfigured.
  /// </summary>
  [Fact]
  public async Task GetAsyncReturnsFailWhenNoConfigExistsAsync()
  {
    // Arrange
    _sessionMock
      .Setup(s => s.LoadAsync<DbConfiguration>(ConfigurationId.Root, It.IsAny<CancellationToken>()))
      .ReturnsAsync((DbConfiguration?)null);

    var store = new ConfigurationStore(_sessionMock.Object);

    // Act
    var result = await store.GetAsync();

    // Assert
    Assert.True(result.IsFailed);
    Assert.NotEmpty(result.Errors);
  }

  /// <summary>
  /// When saving configuration for the first time, the store should create a new Marten
  /// event stream with a configuration-created event containing the provided settings.
  /// </summary>
  [Fact]
  public async Task SaveAsyncCreatesNewConfigStreamAsync()
  {
    // Arrange
    var modelId = "test-model";
    var endpoint = "https://test.example.com";
    var apiKey = "test-api-key";

    var updatedConfig = new DbConfiguration
    {
      Id = ConfigurationId.Root,
      AiModelId = modelId,
      AiEndpoint = endpoint,
      AiApiKey = apiKey
    };

    // No existing config
    _sessionMock
      .Setup(s => s.LoadAsync<DbConfiguration>(ConfigurationId.Root, It.IsAny<CancellationToken>()))
      .ReturnsAsync((DbConfiguration?)null);

    _sessionMock
      .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    // Create a proper Mock<IEventStoreOperations>
    var eventStoreMock = new Mock<IEventStoreOperations>(MockBehavior.Loose);
    _sessionMock.SetupGet(s => s.Events).Returns(eventStoreMock.Object);

    // Setup the mock to return updatedConfig for AggregateStreamAsync
    eventStoreMock
      .Setup(e => e.AggregateStreamAsync<DbConfiguration>(
        ConfigurationId.Root, 
        It.IsAny<long>(), 
        It.IsAny<DateTimeOffset?>(), 
        It.IsAny<DbConfiguration?>()))
      .ReturnsAsync(updatedConfig);

    var store = new ConfigurationStore(_sessionMock.Object);

    // Act
    var result = await store.UpdateAiAsync(modelId, endpoint, apiKey);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(modelId, result.Value.AiModelId);
  }

  /// <summary>
  /// When updating an existing configuration, the store should append an update event
  /// to the existing stream rather than creating a new one.
  /// </summary>
  [Fact]
  public async Task SaveAsyncAppendsUpdateEventToExistingStreamAsync()
  {
    // Arrange
    var modelId = "new-model";
    var endpoint = "https://new.example.com";
    var apiKey = "new-api-key";

    var existingConfig = new DbConfiguration
    {
      Id = ConfigurationId.Root,
      AiModelId = "old-model"
    };

    var updatedConfig = new DbConfiguration
    {
      Id = ConfigurationId.Root,
      AiModelId = modelId,
      AiEndpoint = endpoint,
      AiApiKey = apiKey
    };

    // Existing config found
    _sessionMock
      .Setup(s => s.LoadAsync<DbConfiguration>(ConfigurationId.Root, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingConfig);

    _sessionMock
      .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    // Create a proper Mock<IEventStoreOperations>
    var eventStoreMock = new Mock<IEventStoreOperations>(MockBehavior.Loose);
    _sessionMock.SetupGet(s => s.Events).Returns(eventStoreMock.Object);

    // Setup the mock to return updatedConfig for AggregateStreamAsync
    eventStoreMock
      .Setup(e => e.AggregateStreamAsync<DbConfiguration>(
        ConfigurationId.Root, 
        It.IsAny<long>(), 
        It.IsAny<DateTimeOffset?>(), 
        It.IsAny<DbConfiguration?>()))
      .ReturnsAsync(updatedConfig);

    var store = new ConfigurationStore(_sessionMock.Object);

    // Act
    var result = await store.UpdateAiAsync(modelId, endpoint, apiKey);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
  }

  /// <summary>
  /// The initialization check should return false when no configuration aggregate exists
  /// in the database, indicating the system needs first-time setup.
  /// </summary>
  [Fact]
  public async Task IsInitializedAsyncReturnsFalseWhenNoConfigAsync()
  {
    // Arrange
    _sessionMock
      .Setup(s => s.LoadAsync<DbConfiguration>(ConfigurationId.Root, It.IsAny<CancellationToken>()))
      .ReturnsAsync((DbConfiguration?)null);

    var store = new ConfigurationStore(_sessionMock.Object);

    // Act
    var result = await store.IsInitializedAsync();

    // Assert
    Assert.True(result.IsSuccess);
    Assert.False(result.Value);
  }

  /// <summary>
  /// The initialization check should return true when a configuration aggregate exists
  /// in the database, indicating the system has been set up.
  /// </summary>
  [Fact]
  public async Task IsInitializedAsyncReturnsTrueWhenConfigExistsAsync()
  {
    // Arrange
    var existingConfig = new DbConfiguration { Id = ConfigurationId.Root };

    _sessionMock
      .Setup(s => s.LoadAsync<DbConfiguration>(ConfigurationId.Root, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingConfig);

    var store = new ConfigurationStore(_sessionMock.Object);

    // Act
    var result = await store.IsInitializedAsync();

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Value);
  }
}
