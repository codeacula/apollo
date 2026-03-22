using Apollo.Application.Configuration;
using Apollo.Core.Configuration;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.Config;

public sealed class GetInitializationStatusQueryTests
{
  /// <summary>
  /// When no configuration exists in the database, the query should return a status
  /// indicating the system has not been initialized and needs first-time setup.
  /// </summary>
  [Fact]
  public async Task HandleReturnsNotInitializedWhenNoConfigExistsAsync()
  {
    var store = new Mock<IConfigurationStore>();
    store.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(ConfigurationData.Empty()));

    var handler = new GetInitializationStatusQueryHandler(store.Object);
    var result = await handler.Handle(new GetInitializationStatusQuery(), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.False(result.Value.IsInitialized);
    Assert.False(result.Value.IsAiConfigured);
    Assert.False(result.Value.IsDiscordConfigured);
    Assert.False(result.Value.IsSuperAdminConfigured);
  }

  /// <summary>
  /// When configuration exists, the query should return a status indicating the system
  /// is initialized, along with details about which subsystems are configured.
  /// </summary>
  [Fact]
  public async Task HandleReturnsInitializedWithDetailsWhenConfigExistsAsync()
  {
    var store = new Mock<IConfigurationStore>();
    store.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(new ConfigurationData
      {
        Id = Guid.NewGuid(),
        AiModelId = "gpt-4",
        AiEndpoint = "https://api.openai.com",
        AiApiKey = "secret",
        DiscordToken = "token123",
        DiscordPublicKey = "pubkey123",
        SuperAdminDiscordUserId = "userid123",
      }));

    var handler = new GetInitializationStatusQueryHandler(store.Object);
    var result = await handler.Handle(new GetInitializationStatusQuery(), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.True(result.Value.IsInitialized);
    Assert.True(result.Value.IsAiConfigured);
    Assert.True(result.Value.IsDiscordConfigured);
    Assert.True(result.Value.IsSuperAdminConfigured);
  }

  /// <summary>
  /// When AI configuration fields (ModelId, Endpoint) are empty or missing in the stored
  /// config, the status should report AI as not configured so the health endpoint and
  /// Client can display this information.
  /// </summary>
  [Fact]
  public async Task HandleReportsAiNotConfiguredWhenAiConfigEmptyAsync()
  {
    var store = new Mock<IConfigurationStore>();
    store.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(new ConfigurationData
      {
        Id = Guid.NewGuid(),
        // AI fields intentionally omitted/null
        DiscordToken = "token123",
        DiscordPublicKey = "pubkey123",
      }));

    var handler = new GetInitializationStatusQueryHandler(store.Object);
    var result = await handler.Handle(new GetInitializationStatusQuery(), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.False(result.Value.IsAiConfigured);
    Assert.True(result.Value.IsDiscordConfigured);
  }
}

