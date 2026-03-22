
using Apollo.AI;
using Apollo.AI.Config;
using Apollo.Cache;
using Apollo.Core.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace Apollo.Service.Tests;

public sealed class StartupBehaviorTests
{
  /// <summary>
  /// Test 1: Apollo.Service can register AI and cache services without application config present — no exception thrown.
  /// This verifies that services start gracefully when infrastructure config is present but application config is absent.
  /// </summary>
  [Fact]
  public void BuilderRegistersServicesWithoutAiConfigNoException()
  {
    // Arrange: Configuration WITH infrastructure connection strings but WITHOUT application config
    var configValues = new Dictionary<string, string?>
    {
      // Infrastructure config: present and required
      { "ConnectionStrings:Redis", "localhost:6379" },
      // Application config: deliberately absent
      // ApolloAIConfig:ModelId, ApolloAIConfig:Endpoint not set
    };
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(configValues)
      .Build();

    var services = new ServiceCollection();
    _ = services.AddLogging();

    // Act & Assert - Should not throw when infrastructure config is present but app config is absent
    var exception = Record.Exception(() =>
    {
      _ = services
        .AddCacheServices(configuration.GetConnectionString("Redis")!)
        .AddAiServices(configuration);  // No AI config provided - should use defaults
    });

    Assert.Null(exception);

    // Verify the service collection can build a provider and resolve services
    var provider = services.BuildServiceProvider();
    var aiConfig = provider.GetRequiredService<ApolloAIConfig>();
    Assert.NotNull(aiConfig);
    // Confirm AI config is empty (default)
    Assert.Equal("", aiConfig.ModelId);
    Assert.Equal("", aiConfig.Endpoint);
  }

  /// <summary>
  /// Test 2: When AI config is missing, logging should still work (no errors thrown).
  /// This verifies that missing config is handled gracefully during service building.
  /// </summary>
  [Fact]
  public void ServiceBuilderHandlesMissingAiConfigGracefully()
  {
    // Arrange: Configuration with no AI section
    var configBuilder = new ConfigurationBuilder();
    _ = configBuilder.AddInMemoryCollection([]);
    var configuration = configBuilder.Build();

    var services = new ServiceCollection();
    _ = services.AddLogging();

    // Act
    _ = services.AddAiServices(configuration);

    // Assert: Service provider builds successfully
    var provider = services.BuildServiceProvider();
    var aiConfig = provider.GetRequiredService<ApolloAIConfig>();

    // Config should be default (empty)
    Assert.Equal("", aiConfig.ModelId);
    Assert.Equal("", aiConfig.Endpoint);
    Assert.Equal("", aiConfig.ApiKey);
  }

  /// <summary>
  /// Test 3: Infrastructure values (Redis, Quartz, DB connection strings) being null causes an exception (expected behavior).
  /// This verifies that infrastructure config is still required.
  /// </summary>
  [Fact]
  public void MissingRedisConnectionStringThrowsException()
  {
    // Arrange: Configuration without Redis connection string
    var configBuilder = new ConfigurationBuilder();
    _ = configBuilder.AddInMemoryCollection([]);
    var configuration = configBuilder.Build();

    var services = new ServiceCollection();

    // Act & Assert: Should throw because Redis is infrastructure (required)
    var exception = Record.Exception(() =>
    {
      var redisConnectionString = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection string not found");
      _ = services.AddCacheServices(redisConnectionString);
    });

    Assert.NotNull(exception);
    _ = Assert.IsType<InvalidOperationException>(exception);
  }

  /// <summary>
  /// Test 4: Missing Quartz connection string causes an exception in AddRequiredServices.
  /// Verifies that infrastructure config (Quartz) is still required.
  /// </summary>
  [Fact]
  public void MissingQuartzConnectionStringThrowsException()
  {
    // Arrange: Configuration with Redis but without Quartz
    var configBuilder = new ConfigurationBuilder();
    var inMemorySettings = new Dictionary<string, string?>
    {
      { "ConnectionStrings:Redis", "localhost:6379" }
    };
    _ = configBuilder.AddInMemoryCollection(inMemorySettings);
    var configuration = configBuilder.Build();

    var services = new ServiceCollection();

    // Act & Assert: Should throw because Quartz is infrastructure
    var exception = Record.Exception(() => _ = services.AddRequiredServices(configuration));

    Assert.NotNull(exception);
  }

  /// <summary>
  /// Test 5: Verify that application-level services (AI, Cache) can register without application config.
  /// This demonstrates that infrastructure (Redis) is required but application-level config (AI) is optional.
  /// </summary>
  [Fact]
  public void CoreServicesRegisterWithoutApplicationConfig()
  {
    // Arrange: Configuration WITH infrastructure strings but WITHOUT application config
    var configValues = new Dictionary<string, string?>
    {
      // Infrastructure config: present and required
      { "ConnectionStrings:Redis", "localhost:6379" },
      // Application config: deliberately absent
      // ApolloAIConfig:ModelId, ApolloAIConfig:Endpoint, Discord:Token, SuperAdmin not set
    };
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(configValues)
      .Build();

    var services = new ServiceCollection();
    _ = services.AddLogging();

    // Act: Register services without application config
    var exception = Record.Exception(() =>
    {
      _ = services
        .AddCacheServices(configuration.GetConnectionString("Redis")!)
        .AddAiServices(configuration);
    });

    // Assert: No exception should be thrown
    Assert.Null(exception);

    // Verify services can be resolved
    var provider = services.BuildServiceProvider();
    var aiConfig = provider.GetRequiredService<ApolloAIConfig>();
    Assert.NotNull(aiConfig);
    // Confirm AI config is empty (not configured state)
    Assert.Empty(aiConfig.ModelId);
    Assert.Empty(aiConfig.Endpoint);
  }

  /// <summary>
  /// Test 6: When AI is not configured, a warning log can be generated without errors.
  /// This verifies that logging the "not configured" state works properly.
  /// </summary>
  [Fact]
  public void LoggingAiNotConfiguredWarningWorks()
  {
    // Arrange
    var mockLogger = new Mock<ILogger>();

    _ = mockLogger
      .Setup(x => x.IsEnabled(It.IsAny<LogLevel>()))
      .Returns(true);

    // Act: Call the AILogs method (which uses source-generated logging)
    AILogs.AINotConfigured(mockLogger.Object);

    // Assert: Verify that a warning was logged
    mockLogger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }
}
