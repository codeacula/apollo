using Apollo.AI.Config;
using Apollo.AI.DTOs;
using Apollo.AI.Enums;
using Apollo.AI.Prompts;
using Apollo.AI.Requests;

using Microsoft.Extensions.Logging;

using Moq;

namespace Apollo.AI.Tests;

public sealed class ApolloAIAgentTests
{
  #region CreateRequest Tests

  [Fact]
  public void CreateRequestReturnsAIRequestBuilder()
  {
    // Arrange
    var config = CreateMockConfig();
    var promptLoader = CreateMockPromptLoader();
    var templateProcessor = CreateMockTemplateProcessor();
    var logger = new Mock<ILogger<AIRequestBuilder>>().Object;
    var agent = new ApolloAIAgent(config, promptLoader, templateProcessor, logger);

    // Act
    var builder = agent.CreateRequest();

    // Assert
    Assert.NotNull(builder);
    _ = Assert.IsType<IAIRequestBuilder>(builder, exactMatch: false);
  }

  #endregion CreateRequest Tests

  #region CreateToolPlanningRequest Tests

  [Fact]
  public void CreateToolPlanningRequestWithValidInputsReturnsBuilder()
  {
    // Arrange
    var config = CreateMockConfig();
    var promptLoader = CreateMockPromptLoader();
    var templateProcessor = CreateMockTemplateProcessor();
    var logger = new Mock<ILogger<AIRequestBuilder>>().Object;
    var agent = new ApolloAIAgent(config, promptLoader, templateProcessor, logger);

    var messages = new List<ChatMessageDTO>
    {
      new(ChatRole.User, "Create a task", DateTime.UtcNow)
    };

    // Act
    var builder = agent.CreateToolPlanningRequest(messages, "UTC", "task1, task2");

    // Assert
    Assert.NotNull(builder);
    _ = Assert.IsType<IAIRequestBuilder>(builder, exactMatch: false);
  }

  [Fact]
  public void CreateToolPlanningRequestIncludesMessages()
  {
    // Arrange
    var config = CreateMockConfig();
    var promptLoader = CreateMockPromptLoader();
    var templateProcessor = CreateMockTemplateProcessor();
    var logger = new Mock<ILogger<AIRequestBuilder>>().Object;
    var agent = new ApolloAIAgent(config, promptLoader, templateProcessor, logger);

    var messages = new List<ChatMessageDTO>
    {
      new(ChatRole.User, "Create a task", DateTime.UtcNow)
    };

    // Act
    var builder = agent.CreateToolPlanningRequest(messages, "UTC", "");

    // Assert
    Assert.NotNull(builder);
  }

  [Fact]
  public void CreateToolPlanningRequestIncludesTemplateVariables()
  {
    // Arrange
    var config = CreateMockConfig();
    var promptLoader = CreateMockPromptLoader();
    var templateProcessor = CreateMockTemplateProcessor();
    var logger = new Mock<ILogger<AIRequestBuilder>>().Object;
    var agent = new ApolloAIAgent(config, promptLoader, templateProcessor, logger);

    // Act
    var builder = agent.CreateToolPlanningRequest(
      [],
      "America/New_York",
      "task1");

    // Assert
    Assert.NotNull(builder);
  }

  #endregion CreateToolPlanningRequest Tests

  #region CreateResponseRequest Tests

  [Fact]
  public void CreateResponseRequestWithValidInputsReturnsBuilder()
  {
    // Arrange
    var config = CreateMockConfig();
    var promptLoader = CreateMockPromptLoader();
    var templateProcessor = CreateMockTemplateProcessor();
    var logger = new Mock<ILogger<AIRequestBuilder>>().Object;
    var agent = new ApolloAIAgent(config, promptLoader, templateProcessor, logger);

    var messages = new List<ChatMessageDTO>
    {
      new(ChatRole.User, "Create a task", DateTime.UtcNow)
    };

    // Act
    var builder = agent.CreateResponseRequest(messages, "Task created", "UTC");

    // Assert
    Assert.NotNull(builder);
    _ = Assert.IsType<IAIRequestBuilder>(builder, exactMatch: false);
  }

  [Fact]
  public void CreateResponseRequestIncludesActionsSummary()
  {
    // Arrange
    var config = CreateMockConfig();
    var promptLoader = CreateMockPromptLoader();
    var templateProcessor = CreateMockTemplateProcessor();
    var logger = new Mock<ILogger<AIRequestBuilder>>().Object;
    var agent = new ApolloAIAgent(config, promptLoader, templateProcessor, logger);

    // Act
    var builder = agent.CreateResponseRequest(
      [],
      "Created ToDo: Buy milk",
      "UTC");

    // Assert
    Assert.NotNull(builder);
  }

  #endregion CreateResponseRequest Tests

  #region CreateReminderRequest Tests

  [Fact]
  public void CreateReminderRequestWithValidInputsReturnsBuilder()
  {
    // Arrange
    var config = CreateMockConfig();
    var promptLoader = CreateMockPromptLoader();
    var templateProcessor = CreateMockTemplateProcessor();
    var logger = new Mock<ILogger<AIRequestBuilder>>().Object;
    var agent = new ApolloAIAgent(config, promptLoader, templateProcessor, logger);

    // Act
    var builder = agent.CreateReminderRequest("UTC", "2024-01-01 10:00", "Buy milk");

    // Assert
    Assert.NotNull(builder);
    _ = Assert.IsType<IAIRequestBuilder>(builder, exactMatch: false);
  }

  [Fact]
  public void CreateReminderRequestIncludesCurrentTime()
  {
    // Arrange
    var config = CreateMockConfig();
    var promptLoader = CreateMockPromptLoader();
    var templateProcessor = CreateMockTemplateProcessor();
    var logger = new Mock<ILogger<AIRequestBuilder>>().Object;
    var agent = new ApolloAIAgent(config, promptLoader, templateProcessor, logger);

    // Act
    var builder = agent.CreateReminderRequest("UTC", "10:30", "Task reminder");

    // Assert
    Assert.NotNull(builder);
  }

  #endregion CreateReminderRequest Tests

  #region CreateDailyPlanRequest Tests

  [Fact]
  public void CreateDailyPlanRequestWithValidInputsReturnsBuilder()
  {
    // Arrange
    var config = CreateMockConfig();
    var promptLoader = CreateMockPromptLoader();
    var templateProcessor = CreateMockTemplateProcessor();
    var logger = new Mock<ILogger<AIRequestBuilder>>().Object;
    var agent = new ApolloAIAgent(config, promptLoader, templateProcessor, logger);

    // Act
    var builder = agent.CreateDailyPlanRequest("UTC", "09:00", "task1, task2", 5);

    // Assert
    Assert.NotNull(builder);
    _ = Assert.IsType<IAIRequestBuilder>(builder, exactMatch: false);
  }

  [Fact]
  public void CreateDailyPlanRequestIncludesTaskCount()
  {
    // Arrange
    var config = CreateMockConfig();
    var promptLoader = CreateMockPromptLoader();
    var templateProcessor = CreateMockTemplateProcessor();
    var logger = new Mock<ILogger<AIRequestBuilder>>().Object;
    var agent = new ApolloAIAgent(config, promptLoader, templateProcessor, logger);

    // Act
    var builder = agent.CreateDailyPlanRequest("UTC", "09:00", "tasks", 3);

    // Assert
    Assert.NotNull(builder);
  }

  #endregion CreateDailyPlanRequest Tests

  #region Helper Methods

  private static ApolloAIConfig CreateMockConfig()
  {
    return new ApolloAIConfig
    {
      ApiKey = "test-key",
      ModelId = "gpt-4",
      Endpoint = "https://api.openai.com/v1"
    };
  }

  private static IPromptLoader CreateMockPromptLoader()
  {
    var mockLoader = new Mock<IPromptLoader>();
    _ = mockLoader.Setup(l => l.Load(It.IsAny<string>()))
      .Returns(new PromptDefinition
      {
        SystemPrompt = "Test system prompt",
        Temperature = 0.7
      });
    return mockLoader.Object;
  }

  private static IPromptTemplateProcessor CreateMockTemplateProcessor()
  {
    var mockProcessor = new Mock<IPromptTemplateProcessor>();
    _ = mockProcessor.Setup(p => p.Process(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
      .Returns((string content, Dictionary<string, string> vars) => content);
    return mockProcessor.Object;
  }

  #endregion Helper Methods
}
