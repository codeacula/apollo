using Apollo.Application.Configuration;
using Apollo.Core.Configuration;

using FluentResults;

using MediatR;

using Moq;

namespace Apollo.Application.Tests.Config;

public sealed class SaveConfigurationCommandTests
{
  /// <summary>
  /// The save configuration handler should persist all provided settings (AI config,
  /// Discord config, PersonConfig) through the config store and return success.
  /// </summary>
  [Fact]
  public async Task HandleSavesConfigurationSuccessfullyAsync()
  {
    var store = new Mock<IConfigurationStore>();
    var mediator = new Mock<IMediator>();
    var expectedConfig = new ConfigurationData
    {
      Id = Guid.NewGuid(),
      AiModelId = "gpt-4",
      AiEndpoint = "https://api.openai.com",
      AiApiKey = "secret",
    };
    _ = store.Setup(x => x.UpdateAiAsync("gpt-4", "https://api.openai.com", "secret", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(expectedConfig));

    var handler = new UpdateAiConfigurationCommandHandler(store.Object, mediator.Object);
    var result = await handler.Handle(
      new UpdateAiConfigurationCommand("gpt-4", "https://api.openai.com", "secret"),
      CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("gpt-4", result.Value.AiModelId);
  }

  /// <summary>
  /// During initial setup, the handler should designate the specified Discord user ID
  /// as the super admin by including it in the saved configuration.
  /// </summary>
  [Fact]
  public async Task HandleDesignatesSuperAdminDuringInitialSetupAsync()
  {
    var store = new Mock<IConfigurationStore>();
    var mediator = new Mock<IMediator>();
    var expectedConfig = new ConfigurationData
    {
      Id = Guid.NewGuid(),
      SuperAdminDiscordUserId = "discord-user-123",
    };
    _ = store.Setup(x => x.UpdateSuperAdminAsync("discord-user-123", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(expectedConfig));

    var handler = new UpdateSuperAdminConfigurationCommandHandler(store.Object, mediator.Object);
    var result = await handler.Handle(
      new UpdateSuperAdminConfigurationCommand("discord-user-123"),
      CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("discord-user-123", result.Value.SuperAdminDiscordUserId);
  }

  /// <summary>
  /// When the config store returns a failure (e.g., database error), the handler should
  /// propagate that failure result rather than throwing an exception.
  /// </summary>
  [Fact]
  public async Task HandleReturnsFailureWhenStoreFailsAsync()
  {
    var store = new Mock<IConfigurationStore>();
    var mediator = new Mock<IMediator>();
    _ = store.Setup(x => x.UpdateAiAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<ConfigurationData>("Database connection failed"));

    var handler = new UpdateAiConfigurationCommandHandler(store.Object, mediator.Object);
    var result = await handler.Handle(
      new UpdateAiConfigurationCommand("gpt-4", "https://api.openai.com", null),
      CancellationToken.None);

    Assert.True(result.IsFailed);
    Assert.Contains(result.Errors, e => e.Message.Contains("Database connection failed"));
  }
}

