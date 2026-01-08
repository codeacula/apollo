using Apollo.AI.DTOs;

using Moq;

namespace Apollo.AI.Tests;

public class ApolloReminderMessageGeneratorTests
{
  private readonly Mock<IApolloAIAgent> _mockApolloAIAgent;
  private readonly ApolloReminderMessageGenerator _generator;

  public ApolloReminderMessageGeneratorTests()
  {
    _mockApolloAIAgent = new Mock<IApolloAIAgent>();
    _generator = new ApolloReminderMessageGenerator(_mockApolloAIAgent.Object);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncReturnsAIGeneratedMessageWhenSuccessfulAsync()
  {
    // Arrange
    const string personName = "TestUser";
    var toDoDescriptions = new List<string> { "Buy groceries", "Walk the dog" };
    const string expectedMessage = "Hey TestUser! *gentle purr* Just a friendly nudge - you have a couple of tasks waiting:\n- Buy groceries\n- Walk the dog\n\nYou've got this! 🐾";

    _ = _mockApolloAIAgent
      .Setup(x => x.ChatAsync(It.IsAny<ChatCompletionRequestDTO>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedMessage);

    // Act
    var result = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedMessage, result.Value);
    _mockApolloAIAgent.Verify(
      x => x.ChatAsync(
        It.Is<ChatCompletionRequestDTO>(req =>
          req.Messages.Any(m => m.Content.Contains(personName)) &&
          req.Messages.Any(m => m.Content.Contains("Buy groceries")) &&
          req.Messages.Any(m => m.Content.Contains("Walk the dog"))),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncCallsAIWithCorrectSystemPromptAsync()
  {
    // Arrange
    const string personName = "Alice";
    var toDoDescriptions = new List<string> { "Complete project report" };

    _ = _mockApolloAIAgent
      .Setup(x => x.ChatAsync(It.IsAny<ChatCompletionRequestDTO>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync("Reminder message");

    // Act
    _ = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    _mockApolloAIAgent.Verify(
      x => x.ChatAsync(
        It.Is<ChatCompletionRequestDTO>(req =>
          req.SystemMessage.Contains("Apollo") &&
          req.SystemMessage.Contains("friendly") &&
          req.SystemMessage.Contains("reminder")),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncReturnsFailureWhenAIThrowsExceptionAsync()
  {
    // Arrange
    const string personName = "TestUser";
    var toDoDescriptions = new List<string> { "Buy groceries" };

    _ = _mockApolloAIAgent
      .Setup(x => x.ChatAsync(It.IsAny<ChatCompletionRequestDTO>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("AI service unavailable"));

    // Act
    var result = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Failed to generate reminder message", result.Errors[0].Message);
    Assert.Contains("AI service unavailable", result.Errors[0].Message);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncHandlesMultipleToDosCorrectlyAsync()
  {
    // Arrange
    const string personName = "Bob";
    var toDoDescriptions = new List<string>
    {
      "Task 1",
      "Task 2",
      "Task 3",
      "Task 4"
    };

    _ = _mockApolloAIAgent
      .Setup(x => x.ChatAsync(It.IsAny<ChatCompletionRequestDTO>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync("Multi-task reminder");

    // Act
    var result = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    _mockApolloAIAgent.Verify(
      x => x.ChatAsync(
        It.Is<ChatCompletionRequestDTO>(req =>
          req.Messages.Any(m =>
            m.Content.Contains("Task 1") &&
            m.Content.Contains("Task 2") &&
            m.Content.Contains("Task 3") &&
            m.Content.Contains("Task 4") &&
            m.Content.Contains(", "))),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncHandlesEmptyToDoListGracefullyAsync()
  {
    // Arrange
    const string personName = "Charlie";
    var toDoDescriptions = new List<string>();

    _ = _mockApolloAIAgent
      .Setup(x => x.ChatAsync(It.IsAny<ChatCompletionRequestDTO>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync("No tasks reminder");

    // Act
    var result = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("No tasks reminder", result.Value);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncIncludesPersonNameInRequestAsync()
  {
    // Arrange
    const string personName = "SpecialUser123";
    var toDoDescriptions = new List<string> { "Some task" };

    _ = _mockApolloAIAgent
      .Setup(x => x.ChatAsync(It.IsAny<ChatCompletionRequestDTO>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync("Message");

    // Act
    _ = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    _mockApolloAIAgent.Verify(
      x => x.ChatAsync(
        It.Is<ChatCompletionRequestDTO>(req =>
          req.Messages.Any(m => m.Content.Contains("SpecialUser123"))),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncTrimsQuotesFromResponseAsync()
  {
    // Arrange
    const string personName = "TestUser";
    var toDoDescriptions = new List<string> { "Buy groceries" };
    const string quotedResponse = "\"Hey there! Don't forget your tasks!\"";

    _ = _mockApolloAIAgent
      .Setup(x => x.ChatAsync(It.IsAny<ChatCompletionRequestDTO>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(quotedResponse);

    // Act
    var result = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Hey there! Don't forget your tasks!", result.Value);
    Assert.False(result.Value.StartsWith('"'));
    Assert.False(result.Value.EndsWith('"'));
  }
}
