using Apollo.Application.Configuration;

using MediatR;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace Apollo.API.Tests.Controllers;

public sealed class SetupControllerTests(WebApplicationFactory<IApolloAPI> factory) : IClassFixture<WebApplicationFactory<IApolloAPI>>
{
  private readonly WebApplicationFactory<IApolloAPI> _factory = factory;

  [Theory]
  [InlineData(false, false, false, false)]
  [InlineData(true, true, false, true)]
  public async Task GetStatusReturnsExpectedInitializationStateAsync(
    bool isInitialized,
    bool isAiConfigured,
    bool isDiscordConfigured,
    bool isSuperAdminConfigured)
  {
    var mockMediator = new Mock<IMediator>();
    var status = new InitializationStatus(isInitialized, isAiConfigured, isDiscordConfigured, isSuperAdminConfigured);

    _ = mockMediator
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(status));

    var client = CreateClient(mockMediator);
    var response = await client.GetAsync("/api/setup/status");

    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains($"\"isInitialized\":{isInitialized.ToString().ToLowerInvariant()}", content);
    Assert.Contains($"\"isAiConfigured\":{isAiConfigured.ToString().ToLowerInvariant()}", content);
    Assert.Contains($"\"isDiscordConfigured\":{isDiscordConfigured.ToString().ToLowerInvariant()}", content);
    Assert.Contains($"\"isSuperAdminConfigured\":{isSuperAdminConfigured.ToString().ToLowerInvariant()}", content);
  }

  [Fact]
  public async Task PostSetupAcceptsNestedConfigurationPayloadAsync()
  {
    var mockMediator = new Mock<IMediator>();
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
      ai = new
      {
        modelId = "gpt-4",
        endpoint = "https://api.openai.com",
        apiKey = "key123",
      },
      discord = new
      {
        token = "token123",
        publicKey = "pubkey123",
      },
      superAdmin = new
      {
        discordUserId = "123456789",
      },
    };

    var response = await client.PostAsync("/api/setup", CreateJsonContent(setupRequest));

    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.Contains("\"message\":\"Setup completed successfully.\"", responseContent);
    Assert.Contains("\"isInitialized\":true", responseContent);
  }

  [Fact]
  public async Task PostSetupRejectsEmptyPayloadAsync()
  {
    var mockMediator = new Mock<IMediator>();
    var notInitializedStatus = new InitializationStatus(false, false, false, false);

    _ = mockMediator
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(notInitializedStatus));

    var client = CreateClient(mockMediator);
    var response = await client.PostAsync("/api/setup", CreateJsonContent(new { }));

    Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    var responseContent = await response.Content.ReadAsStringAsync();
    Assert.Contains("At least one setup configuration section is required", responseContent);
  }

  [Fact]
  public async Task PostSetupRejectsWhenAlreadyInitializedAsync()
  {
    var mockMediator = new Mock<IMediator>();
    var initializedStatus = new InitializationStatus(true, true, true, true);

    _ = mockMediator
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(FluentResults.Result.Ok(initializedStatus));

    var client = CreateClient(mockMediator);
    var response = await client.PostAsync("/api/setup", CreateJsonContent(new
    {
      aiModelId = "gpt-4",
      aiEndpoint = "https://api.openai.com",
    }));

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
