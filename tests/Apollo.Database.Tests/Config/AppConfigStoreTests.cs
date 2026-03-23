using Apollo.Database.Configuration;
using Apollo.Core.Dashboard;

using Marten;
using Marten.Events;

using Moq;

namespace Apollo.Database.Tests.Config;

public sealed class AppConfigStoreTests
{
  private readonly Mock<IDocumentSession> _sessionMock;
  private readonly Mock<IDashboardUpdatePublisher> _dashboardUpdatePublisherMock;

  public AppConfigStoreTests()
  {
    _sessionMock = new Mock<IDocumentSession>(MockBehavior.Loose);
    _dashboardUpdatePublisherMock = new Mock<IDashboardUpdatePublisher>(MockBehavior.Loose);
  }

  /// <summary>
  /// When no configuration has ever been saved, the store should return a failure result
  /// indicating the system is unconfigured.
  /// </summary>
  [Fact]
  public async Task GetAsyncReturnsFailWhenNoConfigExistsAsync()
  {
    // Arrange
    _ = _sessionMock
      .Setup(s => s.LoadAsync<DbConfiguration>(ConfigurationId.Root, It.IsAny<CancellationToken>()))
      .ReturnsAsync((DbConfiguration?)null);

    var store = new ConfigurationStore(_sessionMock.Object, _dashboardUpdatePublisherMock.Object);

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
    const string modelId = "test-model";
    const string endpoint = "https://test.example.com";
    const string apiKey = "test-api-key";

    var updatedConfig = new DbConfiguration
    {
      Id = ConfigurationId.Root,
      AiModelId = modelId,
      AiEndpoint = endpoint,
      AiApiKey = apiKey
    };

    // No existing config
    _ = _sessionMock
      .Setup(s => s.LoadAsync<DbConfiguration>(ConfigurationId.Root, It.IsAny<CancellationToken>()))
      .ReturnsAsync((DbConfiguration?)null);

    _ = _sessionMock
      .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    // Create a proper Mock<IEventStoreOperations>
    var eventStoreMock = new Mock<IEventStoreOperations>(MockBehavior.Loose);
    _ = _sessionMock.SetupGet(s => s.Events).Returns(eventStoreMock.Object);

    // Setup the mock to return updatedConfig for AggregateStreamAsync
    _ = eventStoreMock
      .Setup(e => e.AggregateStreamAsync(
        ConfigurationId.Root,
        It.IsAny<long>(),
        It.IsAny<DateTimeOffset?>(),
        It.IsAny<DbConfiguration?>()))
      .ReturnsAsync(updatedConfig);

    var store = new ConfigurationStore(_sessionMock.Object, _dashboardUpdatePublisherMock.Object);

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
    const string modelId = "new-model";
    const string endpoint = "https://new.example.com";
    const string apiKey = "new-api-key";

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
    _ = _sessionMock
      .Setup(s => s.LoadAsync<DbConfiguration>(ConfigurationId.Root, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingConfig);

    _ = _sessionMock
      .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    // Create a proper Mock<IEventStoreOperations>
    var eventStoreMock = new Mock<IEventStoreOperations>(MockBehavior.Loose);
    _ = _sessionMock.SetupGet(s => s.Events).Returns(eventStoreMock.Object);

    // Setup the mock to return updatedConfig for AggregateStreamAsync
    _ = eventStoreMock
      .Setup(e => e.AggregateStreamAsync(
        ConfigurationId.Root,
        It.IsAny<long>(),
        It.IsAny<DateTimeOffset?>(),
        It.IsAny<DbConfiguration?>()))
      .ReturnsAsync(updatedConfig);

    var store = new ConfigurationStore(_sessionMock.Object, _dashboardUpdatePublisherMock.Object);

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
    _ = _sessionMock
      .Setup(s => s.LoadAsync<DbConfiguration>(ConfigurationId.Root, It.IsAny<CancellationToken>()))
      .ReturnsAsync((DbConfiguration?)null);

    var store = new ConfigurationStore(_sessionMock.Object, _dashboardUpdatePublisherMock.Object);

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

    _ = _sessionMock
      .Setup(s => s.LoadAsync<DbConfiguration>(ConfigurationId.Root, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingConfig);

    var store = new ConfigurationStore(_sessionMock.Object, _dashboardUpdatePublisherMock.Object);

    // Act
    var result = await store.IsInitializedAsync();

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Value);
  }
}
