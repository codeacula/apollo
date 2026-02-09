using Apollo.AI;
using Apollo.AI.DTOs;
using Apollo.AI.Requests;
using Apollo.Application.Conversations;
using Apollo.Core.Conversations;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.Models;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

using MediatR;

using Microsoft.Extensions.Logging;

using Moq;

namespace Apollo.Application.Tests.Conversations;

public class ProcessIncomingMessageCommandHandlerTests
{
  private readonly Mock<IApolloAIAgent> _mockAIAgent;
  private readonly Mock<IConversationStore> _mockConversationStore;
  private readonly Mock<IFuzzyTimeParser> _mockFuzzyTimeParser;
  private readonly Mock<ILogger<ProcessIncomingMessageCommandHandler>> _mockLogger;
  private readonly Mock<IMediator> _mockMediator;
  private readonly Mock<IPersonStore> _mockPersonStore;
  private readonly Mock<IToDoStore> _mockToDoStore;
  private readonly Mock<IAIRequestBuilder> _mockRequestBuilder;
  private readonly PersonConfig _personConfig;
  private readonly TimeProvider _timeProvider;
  private readonly ProcessIncomingMessageCommandHandler _handler;

  public ProcessIncomingMessageCommandHandlerTests()
  {
    _mockAIAgent = new Mock<IApolloAIAgent>();
    _mockConversationStore = new Mock<IConversationStore>();
    _mockFuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    _mockLogger = new Mock<ILogger<ProcessIncomingMessageCommandHandler>>();
    _mockMediator = new Mock<IMediator>();
    _mockPersonStore = new Mock<IPersonStore>();
    _mockToDoStore = new Mock<IToDoStore>();
    _mockRequestBuilder = new Mock<IAIRequestBuilder>();
    _personConfig = new PersonConfig { DefaultDailyTaskCount = 5 };
    _timeProvider = TimeProvider.System;

    _handler = new ProcessIncomingMessageCommandHandler(
      _mockAIAgent.Object,
      _mockConversationStore.Object,
      _mockFuzzyTimeParser.Object,
      _mockLogger.Object,
      _mockMediator.Object,
      _mockPersonStore.Object,
      _mockToDoStore.Object,
      _personConfig,
      _timeProvider
    );
  }

  [Fact]
  public async Task HandleWithValidInputCreatesConversationAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var person = CreatePerson(personId);
    var conversationId = new ConversationId(Guid.NewGuid());
    var conversation = CreateConversation(conversationId, personId);
    var command = new ProcessIncomingMessageCommand(personId, new Content("Hello"));

    _ = _mockPersonStore.Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(person));

    _ = _mockConversationStore.Setup(x => x.GetOrCreateConversationByPersonIdAsync(personId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(conversation));

    _ = _mockConversationStore.Setup(x => x.AddMessageAsync(conversationId, It.IsAny<Content>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(conversation));

    _ = _mockToDoStore.Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok<IEnumerable<Domain.ToDos.Models.ToDo>>([]));

    _ = _mockAIAgent.Setup(x => x.CreateToolPlanningRequest(It.IsAny<IEnumerable<ChatMessageDTO>>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns(_mockRequestBuilder.Object);

    _ = _mockRequestBuilder.Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new AIRequestResult { Success = true, Content = /*lang=json,strict*/ "{\"toolCalls\":[]}" });

    _ = _mockAIAgent.Setup(x => x.CreateResponseRequest(It.IsAny<IEnumerable<ChatMessageDTO>>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns(_mockRequestBuilder.Object);

    _ = _mockConversationStore.Setup(x => x.AddReplyAsync(conversationId, It.IsAny<Content>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(conversation));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    _mockConversationStore.Verify(x => x.GetOrCreateConversationByPersonIdAsync(personId, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWithInvalidPersonIdReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var command = new ProcessIncomingMessageCommand(personId, new Content("Hello"));

    _ = _mockPersonStore.Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Fail<Person>("Person not found"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains($"Failed to load person {personId.Value}", result.Errors.Select(e => e.Message));
    _mockConversationStore.Verify(x => x.GetOrCreateConversationByPersonIdAsync(It.IsAny<PersonId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWithConversationCreationFailureReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var person = CreatePerson(personId);
    var command = new ProcessIncomingMessageCommand(personId, new Content("Hello"));

    _ = _mockPersonStore.Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(person));

    _ = _mockConversationStore.Setup(x => x.GetOrCreateConversationByPersonIdAsync(personId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Fail<Conversation>("Database error"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Unable to fetch conversation.", result.Errors.Select(e => e.Message));
  }

  [Fact]
  public async Task HandleWithAddMessageFailureReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var person = CreatePerson(personId);
    var conversationId = new ConversationId(Guid.NewGuid());
    var conversation = CreateConversation(conversationId, personId);
    var command = new ProcessIncomingMessageCommand(personId, new Content("Hello"));

    _ = _mockPersonStore.Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(person));

    _ = _mockConversationStore.Setup(x => x.GetOrCreateConversationByPersonIdAsync(personId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(conversation));

    _ = _mockConversationStore.Setup(x => x.AddMessageAsync(conversationId, It.IsAny<Content>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Fail<Conversation>("Failed to add message"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Unable to add message to conversation.", result.Errors.Select(e => e.Message));
  }

  [Fact]
  public async Task HandleWithUnhandledExceptionReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var command = new ProcessIncomingMessageCommand(personId, new Content("Hello"));

    _ = _mockPersonStore.Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
        .ThrowsAsync(new InvalidOperationException("Unexpected error"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Unexpected error", result.Errors.Select(e => e.Message));
  }

  private static Person CreatePerson(PersonId personId)
  {
    return new Person
    {
      Id = personId,
      PlatformId = new PlatformId("testuser", "123", Domain.Common.Enums.Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }

  private static Conversation CreateConversation(ConversationId conversationId, PersonId personId)
  {
    return new Conversation
    {
      Id = conversationId,
      PersonId = personId,
      Messages = [],
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
