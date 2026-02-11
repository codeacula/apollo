using System.Text.Json;

using Apollo.AI;
using Apollo.AI.Requests;
using Apollo.Application.ToDos;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.ToDos;

public class GetDailyPlanQueryHandlerTests
{
  [Fact]
  public async Task HandleWithNoActiveToDosReturnsEmptyPlanAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoStore = new Mock<IToDoStore>();
    var personStore = new Mock<IPersonStore>();
    var aiAgent = new Mock<IApolloAIAgent>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var handler = new GetDailyPlanQueryHandler(toDoStore.Object, personStore.Object, aiAgent.Object, timeProvider, personConfig);

    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(Enumerable.Empty<ToDo>().AsEnumerable()));

    // Act
    var result = await handler.Handle(new GetDailyPlanQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value.SuggestedTasks);
    Assert.Equal("You have no active todos! 🎉", result.Value.SelectionRationale);
    Assert.Equal(0, result.Value.TotalActiveTodos);
    toDoStore.Verify(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWithActiveToDosLessEqualToDailyTaskCountReturnsAllToDosAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoStore = new Mock<IToDoStore>();
    var personStore = new Mock<IPersonStore>();
    var aiAgent = new Mock<IApolloAIAgent>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var handler = new GetDailyPlanQueryHandler(toDoStore.Object, personStore.Object, aiAgent.Object, timeProvider, personConfig);

    var todos = CreateToDos(personId, 3);
    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(todos.AsEnumerable()));

    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreatePerson(personId, dailyTaskCount: 5)));

    // Act
    var result = await handler.Handle(new GetDailyPlanQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(3, result.Value.SuggestedTasks.Count);
    Assert.Equal(3, result.Value.TotalActiveTodos);
    Assert.Contains("3 active todos", result.Value.SelectionRationale);
  }

  [Fact]
  public async Task HandleWithActiveToDosGreaterThanDailyTaskCountCallsAIAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoStore = new Mock<IToDoStore>();
    var personStore = new Mock<IPersonStore>();
    var aiAgent = new Mock<IApolloAIAgent>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 3, DefaultTimeZoneId = "UTC" };
    var handler = new GetDailyPlanQueryHandler(toDoStore.Object, personStore.Object, aiAgent.Object, timeProvider, personConfig);

    var todos = CreateToDos(personId, 5);
    var selectedIds = todos.Take(3).Select(t => t.Id.Value.ToString()).ToList();
    var aiResponse = CreateAIResponse(selectedIds, "Focus on urgent items first");

    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(todos.AsEnumerable()));

    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreatePerson(personId, dailyTaskCount: 3)));

    var requestBuilder = new Mock<IAIRequestBuilder>();
    _ = requestBuilder
      .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(new AIRequestResult { Success = true, Content = aiResponse });

    _ = aiAgent
      .Setup(x => x.CreateDailyPlanRequest(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        3))
      .Returns(requestBuilder.Object);

    // Act
    var result = await handler.Handle(new GetDailyPlanQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(3, result.Value.SuggestedTasks.Count);
    Assert.Equal(5, result.Value.TotalActiveTodos);
    Assert.Equal("Focus on urgent items first", result.Value.SelectionRationale);
  }

  [Fact]
  public async Task HandleWithToDoStoreFailureReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoStore = new Mock<IToDoStore>();
    var personStore = new Mock<IPersonStore>();
    var aiAgent = new Mock<IApolloAIAgent>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var handler = new GetDailyPlanQueryHandler(toDoStore.Object, personStore.Object, aiAgent.Object, timeProvider, personConfig);

    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<IEnumerable<ToDo>>("Database error"));

    // Act
    var result = await handler.Handle(new GetDailyPlanQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Database error", result.Errors[0].Message);
  }

  [Fact]
  public async Task HandleWithAIAgentFailureReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoStore = new Mock<IToDoStore>();
    var personStore = new Mock<IPersonStore>();
    var aiAgent = new Mock<IApolloAIAgent>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 3, DefaultTimeZoneId = "UTC" };
    var handler = new GetDailyPlanQueryHandler(toDoStore.Object, personStore.Object, aiAgent.Object, timeProvider, personConfig);

    var todos = CreateToDos(personId, 5);
    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(todos.AsEnumerable()));

    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreatePerson(personId, dailyTaskCount: 3)));

    var requestBuilder = new Mock<IAIRequestBuilder>();
    _ = requestBuilder
      .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(new AIRequestResult { Success = false, ErrorMessage = "API rate limit exceeded" });

    _ = aiAgent
      .Setup(x => x.CreateDailyPlanRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
      .Returns(requestBuilder.Object);

    // Act
    var result = await handler.Handle(new GetDailyPlanQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("API rate limit exceeded", result.Errors[0].Message);
  }

  [Fact]
  public async Task HandleWithInvalidAIResponseFormatReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoStore = new Mock<IToDoStore>();
    var personStore = new Mock<IPersonStore>();
    var aiAgent = new Mock<IApolloAIAgent>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 3, DefaultTimeZoneId = "UTC" };
    var handler = new GetDailyPlanQueryHandler(toDoStore.Object, personStore.Object, aiAgent.Object, timeProvider, personConfig);

    var todos = CreateToDos(personId, 5);
    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(todos.AsEnumerable()));

    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreatePerson(personId, dailyTaskCount: 3)));

    var requestBuilder = new Mock<IAIRequestBuilder>();
    _ = requestBuilder
      .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(new AIRequestResult { Success = true, Content = "invalid json" });

    _ = aiAgent
      .Setup(x => x.CreateDailyPlanRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
      .Returns(requestBuilder.Object);

    // Act
    var result = await handler.Handle(new GetDailyPlanQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid JSON", result.Errors[0].Message);
  }

  [Fact]
  public async Task HandleWithAIResponseMissingSelectedTaskIdsReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoStore = new Mock<IToDoStore>();
    var personStore = new Mock<IPersonStore>();
    var aiAgent = new Mock<IApolloAIAgent>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 3, DefaultTimeZoneId = "UTC" };
    var handler = new GetDailyPlanQueryHandler(toDoStore.Object, personStore.Object, aiAgent.Object, timeProvider, personConfig);

    var todos = CreateToDos(personId, 5);
    var aiResponse = JsonSerializer.Serialize(new { rationale = "test" });

    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(todos.AsEnumerable()));

    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreatePerson(personId, dailyTaskCount: 3)));

    var requestBuilder = new Mock<IAIRequestBuilder>();
    _ = requestBuilder
      .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(new AIRequestResult { Success = true, Content = aiResponse });

    _ = aiAgent
      .Setup(x => x.CreateDailyPlanRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
      .Returns(requestBuilder.Object);

    // Act
    var result = await handler.Handle(new GetDailyPlanQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("selected_task_ids", result.Errors[0].Message);
  }

  [Fact]
  public async Task HandleWithCustomPersonConfigDefaultDailyTaskCountUsesPersonSettingAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoStore = new Mock<IToDoStore>();
    var personStore = new Mock<IPersonStore>();
    var aiAgent = new Mock<IApolloAIAgent>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var handler = new GetDailyPlanQueryHandler(toDoStore.Object, personStore.Object, aiAgent.Object, timeProvider, personConfig);

    var todos = CreateToDos(personId, 8);
    var selectedIds = todos.Take(7).Select(t => t.Id.Value.ToString()).ToList();
    var aiResponse = CreateAIResponse(selectedIds, "Custom task count");

    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(todos.AsEnumerable()));

    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreatePerson(personId, dailyTaskCount: 7)));

    var requestBuilder = new Mock<IAIRequestBuilder>();
    _ = requestBuilder
      .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(new AIRequestResult { Success = true, Content = aiResponse });

    _ = aiAgent
      .Setup(x => x.CreateDailyPlanRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 7))
      .Returns(requestBuilder.Object);

    // Act
    var result = await handler.Handle(new GetDailyPlanQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(7, result.Value.SuggestedTasks.Count);
    aiAgent.Verify(x => x.CreateDailyPlanRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 7), Times.Once);
  }

  [Fact]
  public async Task HandleWithExceptionLogsAndReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoStore = new Mock<IToDoStore>();
    var personStore = new Mock<IPersonStore>();
    var aiAgent = new Mock<IApolloAIAgent>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var handler = new GetDailyPlanQueryHandler(toDoStore.Object, personStore.Object, aiAgent.Object, timeProvider, personConfig);

    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("Unexpected error"));

    // Act
    var result = await handler.Handle(new GetDailyPlanQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Unexpected error", result.Errors[0].Message);
  }

  [Fact]
  public async Task HandleWithMultipleToDosDueOnSameDateIncludesAllAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoStore = new Mock<IToDoStore>();
    var personStore = new Mock<IPersonStore>();
    var aiAgent = new Mock<IApolloAIAgent>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var handler = new GetDailyPlanQueryHandler(toDoStore.Object, personStore.Object, aiAgent.Object, timeProvider, personConfig);

    var todos = CreateToDosWithDueDate(personId, 4, DateTime.UtcNow.AddDays(1));
    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(todos.AsEnumerable()));

    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreatePerson(personId, dailyTaskCount: 5)));

    // Act
    var result = await handler.Handle(new GetDailyPlanQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(4, result.Value.SuggestedTasks.Count);
    Assert.All(result.Value.SuggestedTasks, item => Assert.NotNull(item.DueDate));
  }

  private static List<ToDo> CreateToDos(PersonId personId, int count)
  {
    var todos = new List<ToDo>();
    for (int i = 0; i < count; i++)
    {
      todos.Add(new ToDo
      {
        Id = new ToDoId(Guid.NewGuid()),
        PersonId = personId,
        Description = new Description($"Task {i + 1}"),
        Priority = new Priority(Domain.Common.Enums.Level.Blue),
        Energy = new Energy(Domain.Common.Enums.Level.Green),
        Interest = new Interest(Domain.Common.Enums.Level.Yellow),
        CreatedOn = new CreatedOn(DateTime.UtcNow),
        UpdatedOn = new UpdatedOn(DateTime.UtcNow),
        Reminders = []
      });
    }
    return todos;
  }

  private static List<ToDo> CreateToDosWithDueDate(PersonId personId, int count, DateTime dueDate)
  {
    var todos = new List<ToDo>();
    for (int i = 0; i < count; i++)
    {
      todos.Add(new ToDo
      {
        Id = new ToDoId(Guid.NewGuid()),
        PersonId = personId,
        Description = new Description($"Task {i + 1}"),
        Priority = new Priority(Domain.Common.Enums.Level.Blue),
        Energy = new Energy(Domain.Common.Enums.Level.Green),
        Interest = new Interest(Domain.Common.Enums.Level.Yellow),
        DueDate = new DueDate(dueDate),
        CreatedOn = new CreatedOn(DateTime.UtcNow),
        UpdatedOn = new UpdatedOn(DateTime.UtcNow),
        Reminders = []
      });
    }
    return todos;
  }

  private static Person CreatePerson(PersonId personId, int? dailyTaskCount = null)
  {
    return new Person
    {
      Id = personId,
      PlatformId = new PlatformId("testuser", Guid.NewGuid().ToString(), Domain.Common.Enums.Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow),
      DailyTaskCount = dailyTaskCount.HasValue ? new DailyTaskCount(dailyTaskCount.Value) : null
    };
  }

  private static string CreateAIResponse(List<string> selectedIds, string rationale)
  {
    var response = new { selected_task_ids = selectedIds, rationale };
    return JsonSerializer.Serialize(response);
  }
}
