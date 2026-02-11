using Apollo.Application.ToDos;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Microsoft.Extensions.Logging;

using Moq;

namespace Apollo.Application.Tests.ToDos;

public class SetAllToDosAttributeCommandHandlerTests
{
  [Fact]
  public async Task HandleWithValidPriorityUpdatesAllToDosPriorityAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoIds = new[] { new ToDoId(Guid.NewGuid()), new ToDoId(Guid.NewGuid()), new ToDoId(Guid.NewGuid()) };
    var store = new Mock<IToDoStore>();
    var logger = new Mock<ILogger<SetAllToDosAttributeCommandHandler>>();
    var handler = new SetAllToDosAttributeCommandHandler(store.Object, logger.Object);
    var newPriority = new Priority(Level.Red);

    // Setup ownership verification
    foreach (var toDoId in toDoIds)
    {
      var todo = CreateToDo(toDoId, personId);
      _ = store
        .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(todo));

      _ = store
        .Setup(x => x.UpdatePriorityAsync(toDoId, newPriority, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok());
    }

    // Act
    var result = await handler.Handle(
      new SetAllToDosAttributeCommand(personId, toDoIds, Priority: newPriority),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(3, result.Value);
    store.Verify(x => x.UpdatePriorityAsync(It.IsAny<ToDoId>(), newPriority, It.IsAny<CancellationToken>()), Times.Exactly(3));
  }

  [Fact]
  public async Task HandleWithValidEnergyUpdatesAllToDosEnergyAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoIds = new[] { new ToDoId(Guid.NewGuid()), new ToDoId(Guid.NewGuid()) };
    var store = new Mock<IToDoStore>();
    var logger = new Mock<ILogger<SetAllToDosAttributeCommandHandler>>();
    var handler = new SetAllToDosAttributeCommandHandler(store.Object, logger.Object);
    var newEnergy = new Energy(Level.Green);

    // Setup ownership verification
    foreach (var toDoId in toDoIds)
    {
      var todo = CreateToDo(toDoId, personId);
      _ = store
        .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(todo));

      _ = store
        .Setup(x => x.UpdateEnergyAsync(toDoId, newEnergy, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok());
    }

    // Act
    var result = await handler.Handle(
      new SetAllToDosAttributeCommand(personId, toDoIds, Energy: newEnergy),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value);
    store.Verify(x => x.UpdateEnergyAsync(It.IsAny<ToDoId>(), newEnergy, It.IsAny<CancellationToken>()), Times.Exactly(2));
  }

  [Fact]
  public async Task HandleWithMultipleAttributesUpdatesAllAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoId = new ToDoId(Guid.NewGuid());
    var store = new Mock<IToDoStore>();
    var logger = new Mock<ILogger<SetAllToDosAttributeCommandHandler>>();
    var handler = new SetAllToDosAttributeCommandHandler(store.Object, logger.Object);
    var newPriority = new Priority(Level.Red);
    var newEnergy = new Energy(Level.Blue);
    var newInterest = new Interest(Level.Yellow);

    var todo = CreateToDo(toDoId, personId);
    _ = store
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(todo));

    _ = store
      .Setup(x => x.UpdatePriorityAsync(toDoId, newPriority, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = store
      .Setup(x => x.UpdateEnergyAsync(toDoId, newEnergy, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = store
      .Setup(x => x.UpdateInterestAsync(toDoId, newInterest, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await handler.Handle(
      new SetAllToDosAttributeCommand(personId, [toDoId], Priority: newPriority, Energy: newEnergy, Interest: newInterest),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(1, result.Value);
    store.Verify(x => x.UpdatePriorityAsync(toDoId, newPriority, It.IsAny<CancellationToken>()), Times.Once);
    store.Verify(x => x.UpdateEnergyAsync(toDoId, newEnergy, It.IsAny<CancellationToken>()), Times.Once);
    store.Verify(x => x.UpdateInterestAsync(toDoId, newInterest, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWithEmptyToDoIdsListFetchesAllActiveToDosAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var store = new Mock<IToDoStore>();
    var logger = new Mock<ILogger<SetAllToDosAttributeCommandHandler>>();
    var handler = new SetAllToDosAttributeCommandHandler(store.Object, logger.Object);
    var newPriority = new Priority(Level.Red);

    var todos = CreateToDos(personId, 2);
    _ = store
      .Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(todos.AsEnumerable()));

    foreach (var todo in todos)
    {
      _ = store
        .Setup(x => x.GetAsync(todo.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(todo));

      _ = store
        .Setup(x => x.UpdatePriorityAsync(todo.Id, newPriority, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok());
    }

    // Act
    var result = await handler.Handle(
      new SetAllToDosAttributeCommand(personId, [], Priority: newPriority),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value);
    store.Verify(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWithUnauthorizedToDoSkipsAndContinuesAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var authorizedId = new ToDoId(Guid.NewGuid());
    var unauthorizedId = new ToDoId(Guid.NewGuid());
    var store = new Mock<IToDoStore>();
    var logger = new Mock<ILogger<SetAllToDosAttributeCommandHandler>>();
    var handler = new SetAllToDosAttributeCommandHandler(store.Object, logger.Object);
    var newPriority = new Priority(Level.Red);

    var authorizedTodo = CreateToDo(authorizedId, personId);
    var unauthorizedTodo = CreateToDo(unauthorizedId, new PersonId(Guid.NewGuid()));

    _ = store
      .Setup(x => x.GetAsync(authorizedId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(authorizedTodo));

    _ = store
      .Setup(x => x.GetAsync(unauthorizedId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(unauthorizedTodo));

    _ = store
      .Setup(x => x.UpdatePriorityAsync(authorizedId, newPriority, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await handler.Handle(
      new SetAllToDosAttributeCommand(personId, [authorizedId, unauthorizedId], Priority: newPriority),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(1, result.Value); // Only authorized todo updated
    store.Verify(x => x.UpdatePriorityAsync(authorizedId, newPriority, It.IsAny<CancellationToken>()), Times.Once);
    store.Verify(x => x.UpdatePriorityAsync(unauthorizedId, It.IsAny<Priority>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWithStoreFetchFailureReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoId = new ToDoId(Guid.NewGuid());
    var store = new Mock<IToDoStore>();
    var logger = new Mock<ILogger<SetAllToDosAttributeCommandHandler>>();
    var handler = new SetAllToDosAttributeCommandHandler(store.Object, logger.Object);
    var newPriority = new Priority(Level.Red);

    _ = store
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<Domain.ToDos.Models.ToDo>("Not found"));

    // Act
    var result = await handler.Handle(
      new SetAllToDosAttributeCommand(personId, [toDoId], Priority: newPriority),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(0, result.Value); // No todos updated
  }

  [Fact]
  public async Task HandleWithPartialUpdateFailureUpdatesSuccessfulOnesAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var successId = new ToDoId(Guid.NewGuid());
    var failureId = new ToDoId(Guid.NewGuid());
    var store = new Mock<IToDoStore>();
    var logger = new Mock<ILogger<SetAllToDosAttributeCommandHandler>>();
    var handler = new SetAllToDosAttributeCommandHandler(store.Object, logger.Object);
    var newPriority = new Priority(Level.Red);

    var successTodo = CreateToDo(successId, personId);
    var failureTodo = CreateToDo(failureId, personId);

    _ = store
      .Setup(x => x.GetAsync(successId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(successTodo));

    _ = store
      .Setup(x => x.GetAsync(failureId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(failureTodo));

    _ = store
      .Setup(x => x.UpdatePriorityAsync(successId, newPriority, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = store
      .Setup(x => x.UpdatePriorityAsync(failureId, newPriority, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail("Database error"));

    // Act
    var result = await handler.Handle(
      new SetAllToDosAttributeCommand(personId, [successId, failureId], Priority: newPriority),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(1, result.Value); // Only successful one counted
    store.Verify(x => x.UpdatePriorityAsync(successId, newPriority, It.IsAny<CancellationToken>()), Times.Once);
    store.Verify(x => x.UpdatePriorityAsync(failureId, newPriority, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWithExceptionReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var toDoId = new ToDoId(Guid.NewGuid());
    var store = new Mock<IToDoStore>();
    var logger = new Mock<ILogger<SetAllToDosAttributeCommandHandler>>();
    var handler = new SetAllToDosAttributeCommandHandler(store.Object, logger.Object);
    var newPriority = new Priority(Level.Red);

    _ = store
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("Unexpected error"));

    // Act
    var result = await handler.Handle(
      new SetAllToDosAttributeCommand(personId, [toDoId], Priority: newPriority),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Unexpected error", result.Errors[0].Message);
  }

  [Fact]
  public async Task HandleWithLargeToDoCountUpdatesAllAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var store = new Mock<IToDoStore>();
    var logger = new Mock<ILogger<SetAllToDosAttributeCommandHandler>>();
    var handler = new SetAllToDosAttributeCommandHandler(store.Object, logger.Object);
    var newPriority = new Priority(Level.Red);

    var todos = CreateToDos(personId, 50);
    _ = store
      .Setup(x => x.GetByPersonIdAsync(personId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(todos.AsEnumerable()));

    foreach (var todo in todos)
    {
      _ = store
        .Setup(x => x.GetAsync(todo.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(todo));

      _ = store
        .Setup(x => x.UpdatePriorityAsync(todo.Id, newPriority, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok());
    }

    // Act
    var result = await handler.Handle(
      new SetAllToDosAttributeCommand(personId, [], Priority: newPriority),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(50, result.Value);
  }

  private static Domain.ToDos.Models.ToDo CreateToDo(ToDoId toDoId, PersonId personId)
  {
    return new Domain.ToDos.Models.ToDo
    {
      Id = toDoId,
      PersonId = personId,
      Description = new Description("Test ToDo"),
      Priority = new Priority(Level.Blue),
      Energy = new Energy(Level.Blue),
      Interest = new Interest(Level.Blue),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow),
      Reminders = []
    };
  }

  private static List<Domain.ToDos.Models.ToDo> CreateToDos(PersonId personId, int count)
  {
    var todos = new List<Domain.ToDos.Models.ToDo>();
    for (int i = 0; i < count; i++)
    {
      todos.Add(new Domain.ToDos.Models.ToDo
      {
        Id = new ToDoId(Guid.NewGuid()),
        PersonId = personId,
        Description = new Description($"Test ToDo {i + 1}"),
        Priority = new Priority(Level.Blue),
        Energy = new Energy(Level.Blue),
        Interest = new Interest(Level.Blue),
        CreatedOn = new CreatedOn(DateTime.UtcNow),
        UpdatedOn = new UpdatedOn(DateTime.UtcNow),
        Reminders = []
      });
    }
    return todos;
  }
}
