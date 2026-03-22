namespace Apollo.AI.Tests;

using Apollo.AI.Config;
using Apollo.AI.Prompts;
using Apollo.AI.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

public sealed class GracefulDegradationTests
{
  /// <summary>
  /// When ApolloAIConfig has empty/default values (no ModelId, no Endpoint), any
  /// AI-dependent operation should return a clear Result.Fail with a descriptive
  /// "AI not configured" message rather than throwing a URI format exception or
  /// other cryptic error.
  /// </summary>
  [Fact]
  public async Task AgentReturnsNotConfiguredErrorWhenConfigIsEmptyAsync()
  {
    // Arrange
    var emptyConfig = new ApolloAIConfig
    {
      ModelId = "",
      Endpoint = "",
      ApiKey = ""
    };

    var mockPromptLoader = new Mock<IPromptLoader>();
    var mockTemplateProcessor = new Mock<IPromptTemplateProcessor>();
    var mockLogger = new Mock<ILogger<AIRequestBuilder>>();

    var builder = new AIRequestBuilder(emptyConfig, mockTemplateProcessor.Object, mockLogger.Object);

    // Act
    var result = await builder.ExecuteAsync();

    // Assert
    Assert.False(result.Success);
    Assert.NotNull(result.ErrorMessage);
    Assert.Contains("not configured", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
  }

  /// <summary>
  /// The AI service registration (AddAiServices) should not throw or crash when
  /// the ApolloAIConfig section is completely absent from configuration. The service
  /// should register with a no-op or deferred implementation.
  /// </summary>
  [Fact]
  public void AiServiceRegistrationDoesNotCrashWhenConfigMissing()
  {
    // Arrange
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder().Build(); // Empty configuration, no ApolloAIConfig section

    // Act & Assert
    var exception = Record.Exception(() =>
    {
      services.AddAiServices(configuration);
    });

    Assert.Null(exception);

    // Verify the service can be built and resolved
    var serviceProvider = services.BuildServiceProvider();
    var agent = serviceProvider.GetRequiredService<IApolloAIAgent>();

    Assert.NotNull(agent);
  }
}
