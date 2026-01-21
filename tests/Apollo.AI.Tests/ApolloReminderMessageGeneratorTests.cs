using Apollo.AI.DTOs;
using Apollo.AI.Requests;

using Moq;

namespace Apollo.AI.Tests;

public class ApolloReminderMessageGeneratorTests
{
  private readonly Mock<IApolloAIAgent> _mockApolloAIAgent;
  private readonly Mock<IAIRequestBuilder> _mockRequestBuilder;
  private readonly ApolloReminderMessageGenerator _generator;

  private string? _capturedSystemPrompt;
  private ChatMessageDTO? _capturedMessage;

  public ApolloReminderMessageGeneratorTests()
  {
    _mockApolloAIAgent = new Mock<IApolloAIAgent>();
    _mockRequestBuilder = new Mock<IAIRequestBuilder>();
    _generator = new ApolloReminderMessageGenerator(_mockApolloAIAgent.Object);

    SetupDefaultBuilderChain();
  }

  private void SetupDefaultBuilderChain()
  {
    _mockRequestBuilder
      .Setup(x => x.WithSystemPrompt(It.IsAny<string>()))
      .Callback<string>(s => _capturedSystemPrompt = s)
      .Returns(_mockRequestBuilder.Object);

    _mockRequestBuilder
      .Setup(x => x.WithTemperature(It.IsAny<double>()))
      .Returns(_mockRequestBuilder.Object);

    _mockRequestBuilder
      .Setup(x => x.WithMessage(It.IsAny<ChatMessageDTO>()))
      .Callback<ChatMessageDTO>(m => _capturedMessage = m)
      .Returns(_mockRequestBuilder.Object);

    _mockRequestBuilder
      .Setup(x => x.WithToolCalling(It.IsAny<bool>()))
      .Returns(_mockRequestBuilder.Object);

    _mockApolloAIAgent
      .Setup(x => x.CreateRequest())
      .Returns(_mockRequestBuilder.Object);
  }

  private void SetupSuccessResult(string content)
  {
    _mockRequestBuilder
      .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(new AIRequestResult { Success = true, Content = content });
  }

  private void SetupFailureResult(string errorMessage)
  {
    _mockRequestBuilder
      .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(AIRequestResult.Failure(errorMessage));
  }

  private void SetupThrowsException(Exception exception)
  {
    _mockRequestBuilder
      .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
      .ThrowsAsync(exception);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncReturnsAIGeneratedMessageWhenSuccessfulAsync()
  {
    // Arrange
    const string personName = "TestUser";
    var toDoDescriptions = new List<string> { "Buy groceries", "Walk the dog" };
    const string expectedMessage = "Hey TestUser! *gentle purr* Just a friendly nudge - you have a couple of tasks waiting. You've got this!";

    SetupSuccessResult(expectedMessage);

    // Act
    var result = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedMessage, result.Value);
    Assert.NotNull(_capturedMessage);
    Assert.Contains(personName, _capturedMessage!.Content);
    Assert.Contains("Buy groceries", _capturedMessage.Content);
    Assert.Contains("Walk the dog", _capturedMessage.Content);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncCallsAIWithCorrectSystemPromptAsync()
  {
    // Arrange
    const string personName = "Alice";
    var toDoDescriptions = new List<string> { "Complete project report" };

    SetupSuccessResult("Reminder message");

    // Act
    _ = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    Assert.NotNull(_capturedSystemPrompt);
    Assert.Contains("Apollo", _capturedSystemPrompt);
    Assert.Contains("friendly", _capturedSystemPrompt);
    Assert.Contains("reminder", _capturedSystemPrompt);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncReturnsFailureWhenAIThrowsExceptionAsync()
  {
    // Arrange
    const string personName = "TestUser";
    var toDoDescriptions = new List<string> { "Buy groceries" };

    SetupThrowsException(new InvalidOperationException("AI service unavailable"));

    // Act
    var result = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Failed to generate reminder message", result.Errors[0].Message);
    Assert.Contains("AI service unavailable", result.Errors[0].Message);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncReturnsFailureWhenAIReturnsFailureResultAsync()
  {
    // Arrange
    const string personName = "TestUser";
    var toDoDescriptions = new List<string> { "Buy groceries" };

    SetupFailureResult("AI service unavailable");

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

    SetupSuccessResult("Multi-task reminder");

    // Act
    var result = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(_capturedMessage);
    Assert.Contains("Task 1", _capturedMessage!.Content);
    Assert.Contains("Task 2", _capturedMessage.Content);
    Assert.Contains("Task 3", _capturedMessage.Content);
    Assert.Contains("Task 4", _capturedMessage.Content);
    Assert.Contains(", ", _capturedMessage.Content);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncHandlesEmptyToDoListGracefullyAsync()
  {
    // Arrange
    const string personName = "Charlie";
    var toDoDescriptions = new List<string>();

    SetupSuccessResult("No tasks reminder");

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

    SetupSuccessResult("Message");

    // Act
    _ = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    Assert.NotNull(_capturedMessage);
    Assert.Contains("SpecialUser123", _capturedMessage!.Content);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncTrimsQuotesFromResponseAsync()
  {
    // Arrange
    const string personName = "TestUser";
    var toDoDescriptions = new List<string> { "Buy groceries" };
    const string quotedResponse = "\"Hey there! Don't forget your tasks!\"";

    SetupSuccessResult(quotedResponse);

    // Act
    var result = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Hey there! Don't forget your tasks!", result.Value);
    Assert.False(result.Value.StartsWith('"'));
    Assert.False(result.Value.EndsWith('"'));
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncDisablesToolCallingAsync()
  {
    // Arrange
    const string personName = "TestUser";
    var toDoDescriptions = new List<string> { "Buy groceries" };

    SetupSuccessResult("Message");

    // Act
    _ = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    _mockRequestBuilder.Verify(x => x.WithToolCalling(false), Times.Once);
  }

  [Fact]
  public async Task GenerateReminderMessageAsyncSetsTemperatureAsync()
  {
    // Arrange
    const string personName = "TestUser";
    var toDoDescriptions = new List<string> { "Buy groceries" };

    SetupSuccessResult("Message");

    // Act
    _ = await _generator.GenerateReminderMessageAsync(personName, toDoDescriptions, CancellationToken.None);

    // Assert
    _mockRequestBuilder.Verify(x => x.WithTemperature(0.8), Times.Once);
  }
}
