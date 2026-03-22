using Apollo.Application.Configuration;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Apollo.API.Tests.Controllers;

public sealed class SetupControllerTests : IClassFixture<WebApplicationFactory<IApolloAPI>>
{
  private readonly WebApplicationFactory<IApolloAPI> _factory;

  public SetupControllerTests(WebApplicationFactory<IApolloAPI> factory)
  {
    _factory = factory;
  }

  /// <summary>
  /// GET /api/setup/status should return a JSON response indicating the system is
  /// not initialized when no configuration exists in the database.
  /// </summary>
  [Fact]
  public async Task GetStatusReturnsNotInitializedWhenNoConfigAsync()
  {
    // Arrange
    var mockMediator = new Mock<IMediator>();
    var notInitializedStatus = new InitializationStatus(
      IsInitialized: false,
      IsAiConfigured: false,
      IsDiscordConfigured: false,
      IsSuperAdminConfigured: false
    );

    mockMediator
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(notInitializedStatus));

    var factory = _factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureServices(services =>
      {
        services.AddTransient(_ => mockMediator.Object);
      });
    });

    var client = factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/setup/status");

    // Assert
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("\"isInitialized\":false", content);
    Assert.Contains("\"isAiConfigured\":false", content);
    Assert.Contains("\"isDiscordConfigured\":false", content);
    Assert.Contains("\"isSuperAdminConfigured\":false", content);
  }

  /// <summary>
  /// GET /api/setup/status should return a JSON response indicating the system is
  /// initialized when configuration exists, including which subsystems are configured.
  /// </summary>
  [Fact]
  public async Task GetStatusReturnsInitializedWhenConfigExistsAsync()
  {
    // Arrange
    var mockMediator = new Mock<IMediator>();
    var initializedStatus = new InitializationStatus(
      IsInitialized: true,
      IsAiConfigured: true,
      IsDiscordConfigured: false,
      IsSuperAdminConfigured: true
    );

    mockMediator
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(initializedStatus));

    var factory = _factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureServices(services =>
      {
        services.AddTransient(_ => mockMediator.Object);
      });
    });

    var client = factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/setup/status");

    // Assert
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("\"isInitialized\":true", content);
    Assert.Contains("\"isAiConfigured\":true", content);
    Assert.Contains("\"isDiscordConfigured\":false", content);
    Assert.Contains("\"isSuperAdminConfigured\":true", content);
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

    var configData = new Apollo.Core.Configuration.ConfigurationData
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

    mockMediator
      .InSequence(sequence)
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(notInitializedStatus));

    mockMediator
      .InSequence(sequence)
      .Setup(m => m.Send(It.IsAny<UpdateAiConfigurationCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(configData));

    mockMediator
      .InSequence(sequence)
      .Setup(m => m.Send(It.IsAny<UpdateDiscordConfigurationCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(configData));

    mockMediator
      .InSequence(sequence)
      .Setup(m => m.Send(It.IsAny<UpdateSuperAdminConfigurationCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(configData));

    var initializedStatus = new InitializationStatus(true, true, true, true);
    mockMediator
      .InSequence(sequence)
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(initializedStatus));

    var factory = _factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureServices(services =>
      {
        services.AddTransient(_ => mockMediator.Object);
      });
    });

    var client = factory.CreateClient();

    var setupRequest = new
    {
      aiModelId = "gpt-4",
      aiEndpoint = "https://api.openai.com",
      aiApiKey = "key123",
      discordToken = "token123",
      discordPublicKey = "pubkey123",
      superAdminDiscordUserId = "123456789",
    };

    var content = new StringContent(
      System.Text.Json.JsonSerializer.Serialize(setupRequest),
      System.Text.Encoding.UTF8,
      "application/json"
    );

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
    mockMediator
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(initializedStatus));

    var factory = _factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureServices(services =>
      {
        services.AddTransient(_ => mockMediator.Object);
      });
    });

    var client = factory.CreateClient();

    var setupRequest = new
    {
      aiModelId = "gpt-4",
      aiEndpoint = "https://api.openai.com",
    };

    var content = new StringContent(
      System.Text.Json.JsonSerializer.Serialize(setupRequest),
      System.Text.Encoding.UTF8,
      "application/json"
    );

    // Act
    var response = await client.PostAsync("/api/setup", content);

    // Assert
    Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.Contains("already initialized", responseContent);
  }
}
