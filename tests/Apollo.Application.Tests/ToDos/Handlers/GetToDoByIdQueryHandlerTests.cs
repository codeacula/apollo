using Apollo.Application.ToDos.Handlers;
using Apollo.Application.ToDos.Queries;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.ToDos.Handlers;

public class GetToDoByIdQueryHandlerTests
{
  [Fact]
  public async Task HandleReturnsToDoWhenFoundAsync()
  {
    // Arrange
    var toDoStore = new Mock<IToDoStore>();
    var handler = new GetToDoByIdQueryHandler(toDoStore.Object);
    var toDoId = new ToDoId(Guid.NewGuid());
    var expectedToDo = CreateToDo(toDoId);

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(expectedToDo));

    // Act
    var result = await handler.Handle(new GetToDoByIdQuery(toDoId), CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedToDo, result.Value);
    toDoStore.Verify(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleReturnsFailureWhenStoreThrowsExceptionAsync()
  {
    // Arrange
    var toDoStore = new Mock<IToDoStore>();
    var handler = new GetToDoByIdQueryHandler(toDoStore.Object);
    var toDoId = new ToDoId(Guid.NewGuid());

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("boom"));

    // Act
    var result = await handler.Handle(new GetToDoByIdQuery(toDoId), CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Equal("boom", result.Errors[0].Message);
  }

  private static ToDo CreateToDo(ToDoId toDoId)
  {
    return new ToDo
    {
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      Description = new Description("test"),
      Energy = new Energy(Level.Blue),
      Id = toDoId,
      Interest = new Interest(Level.Blue),
      PersonId = new PersonId(Guid.NewGuid()),
      Priority = new Priority(Level.Blue),
      Reminders = [],
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
