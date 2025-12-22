using Apollo.Application.ToDos;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.ToDos;

public class GetToDosByPersonIdQueryHandlerTests
{
  [Fact]
  public async Task HandleReturnsAllToDosForPersonAsync()
  {
    // Arrange
    var toDoStore = new Mock<IToDoStore>();
    var handler = new GetToDosByPersonIdQueryHandler(toDoStore.Object);
    var personId = new PersonId(Guid.NewGuid());
    var toDos = new[] { CreateToDo(personId), CreateToDo(personId) };

    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDo>>(toDos));

    // Act
    var result = await handler.Handle(new GetToDosByPersonIdQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value.Count());
    toDoStore.Verify(x => x.GetByPersonIdAsync(personId, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleReturnsEmptyWhenNoToDosFoundAsync()
  {
    // Arrange
    var toDoStore = new Mock<IToDoStore>();
    var handler = new GetToDosByPersonIdQueryHandler(toDoStore.Object);
    var personId = new PersonId(Guid.NewGuid());

    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDo>>([]));

    // Act
    var result = await handler.Handle(new GetToDosByPersonIdQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value);
  }

  [Fact]
  public async Task HandleReturnsFailureWhenStoreThrowsExceptionAsync()
  {
    // Arrange
    var toDoStore = new Mock<IToDoStore>();
    var handler = new GetToDosByPersonIdQueryHandler(toDoStore.Object);
    var personId = new PersonId(Guid.NewGuid());

    _ = toDoStore
      .Setup(x => x.GetByPersonIdAsync(personId, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("boom"));

    // Act
    var result = await handler.Handle(new GetToDosByPersonIdQuery(personId), CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Equal("boom", result.Errors[0].Message);
  }

  private static ToDo CreateToDo(PersonId personId)
  {
    return new ToDo
    {
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      Description = new Description("test"),
      Energy = new Energy(Level.Blue),
      Id = new ToDoId(Guid.NewGuid()),
      Interest = new Interest(Level.Blue),
      PersonId = personId,
      Priority = new Priority(Level.Blue),
      Reminders = [],
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
