using Apollo.Application.Configuration;

using MediatR;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace Apollo.API.Tests.Controllers;

public sealed class SetupControllerTests(WebApplicationFactory<IApolloAPI> factory) : IClassFixture<WebApplicationFactory<IApolloAPI>>
{
  private readonly WebApplicationFactory<IApolloAPI> _factory = factory;

  /// <summary>
  /// GET /api/setup/status should return a JSON response indicating the system is
  /// not initialized when no configuration exists in the database.
  /// </summary>
  /// <param name="isInitialized"></param>
  /// <param name="isAiConfigured"></param>
  /// <param name="isDiscordConfigured"></param>
  /// <param name="isSuperAdminConfigured"></param>
  [Theory]
  [InlineData(false, false, false, false)]
  [InlineData(true, true, false, true)]
  public async Task GetStatusReturnsExpectedInitializationStateAsync(
    bool isInitialized,
    bool isAiConfigured,
    bool isDiscordConfigured,
    bool isSuperAdminConfigured)
  {
    // Arrange
    var mockMediator = new Mock<IMediator>();
    var status = new InitializationStatus(isInitialized, isAiConfigured, isDiscordConfigured, isSuperAdminConfigured);

    _ = mockMediator
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(status));

    var client = CreateClient(mockMediator);

    // Act
    var response = await client.GetAsync("/api/setup/status");

    // Assert
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains($"\"isInitialized\":{isInitialized.ToString().ToLowerInvariant()}", content);
    Assert.Contains($"\"isAiConfigured\":{isAiConfigured.ToString().ToLowerInvariant()}", content);
    Assert.Contains($"\"isDiscordConfigured\":{isDiscordConfigured.ToString().ToLowerInvariant()}", content);
    Assert.Contains($"\"isSuperAdminConfigured\":{isSuperAdminConfigured.ToString().ToLowerInvariant()}", content);
  }

  /// <summary>
  /// POST /api/setup should accept a configuration payload (AI settings, Discord
  /// settings, super admin Discord user ID) and persist it via MediatR, returning
  /// success on first-time setup.
  /// </summary>
  [Fact]
  public async Task PostSetupSavesConfigurationAsync()
  {
    // Arrange
    var mockMediator = new Mock<IMediator>();

    // Initial status query returns not initialized
    var notInitializedStatus = new InitializationStatus(false, false, false, false);

    var configData = new Core.Configuration.ConfigurationData
    {
      Id = Guid.NewGuid(),
      AiModelId = "gpt-4",
      AiEndpoint = "https://api.openai.com",
      DiscordToken = "token123",
      DiscordPublicKey = "pubkey123",
      SuperAdminDiscordUserId = "123456789",
    };

    // Use a sequential pattern: first call is status check (returns not init),
    // then three config update commands (all return success),
    // then final status check (returns initialized)
    var sequence = new MockSequence();

    _ = mockMediator
      .InSequence(sequence)
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(notInitializedStatus));

    _ = mockMediator
      .InSequence(sequence)
      .Setup(m => m.Send(It.IsAny<UpdateAiConfigurationCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(configData));

    _ = mockMediator
      .InSequence(sequence)
      .Setup(m => m.Send(It.IsAny<UpdateDiscordConfigurationCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(configData));

    _ = mockMediator
      .InSequence(sequence)
      .Setup(m => m.Send(It.IsAny<UpdateSuperAdminConfigurationCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(configData));

    var initializedStatus = new InitializationStatus(true, true, true, true);
    _ = mockMediator
      .InSequence(sequence)
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(initializedStatus));

    var client = CreateClient(mockMediator);

    var setupRequest = new
    {
      aiModelId = "gpt-4",
      aiEndpoint = "https://api.openai.com",
      aiApiKey = "key123",
      discordToken = "token123",
      discordPublicKey = "pubkey123",
      superAdminDiscordUserId = "123456789",
    };

    var content = CreateJsonContent(setupRequest);

    // Act
    var response = await client.PostAsync("/api/setup", content);

    // Assert
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.Contains("\"message\":\"Setup completed successfully.\"", responseContent);
    Assert.Contains("\"isInitialized\":true", responseContent);
  }

  /// <summary>
  /// POST /api/setup should reject the request with an appropriate error when the
  /// system is already initialized. Configuration updates after initial setup must
  /// go through a separate update endpoint.
  /// </summary>
  [Fact]
  public async Task PostSetupRejectsWhenAlreadyInitializedAsync()
  {
    // Arrange
    var mockMediator = new Mock<IMediator>();

    // Status query returns already initialized
    var initializedStatus = new InitializationStatus(true, true, true, true);
    _ = mockMediator
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(initializedStatus));

    var client = CreateClient(mockMediator);

    var setupRequest = new
    {
      aiModelId = "gpt-4",
      aiEndpoint = "https://api.openai.com",
    };

    var content = CreateJsonContent(setupRequest);

    // Act
    var response = await client.PostAsync("/api/setup", content);

    // Assert
    Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.Contains("already initialized", responseContent);
  }

  private HttpClient CreateClient(Mock<IMediator> mediator)
  {
    var factory = _factory.WithWebHostBuilder(builder =>
      _ = builder.ConfigureServices(services => _ = services.AddTransient(_ => mediator.Object)));

    return factory.CreateClient();
  }

  private static StringContent CreateJsonContent(object body)
  {
    return new StringContent(
      System.Text.Json.JsonSerializer.Serialize(body),
      System.Text.Encoding.UTF8,
      "application/json"
    );
  }
}
