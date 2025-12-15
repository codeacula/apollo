using Apollo.Application.ToDos;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.ToDos;

public class UpdateToDoCommandHandlerTests
{
  [Fact]
  public async Task HandleUpdatesToDoSuccessfully()
  {
    // Arrange
    var toDoStore = new Mock<IToDoStore>();
    var handler = new UpdateToDoCommandHandler(toDoStore.Object);
    var toDoId = new ToDoId(Guid.NewGuid());
    var description = new Description("Updated description");

    _ = toDoStore
      .Setup(x => x.UpdateAsync(toDoId, description, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await handler.Handle(new UpdateToDoCommand(toDoId, description), CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    toDoStore.Verify(x => x.UpdateAsync(toDoId, description, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleReturnsFailureWhenStoreThrowsException()
  {
    // Arrange
    var toDoStore = new Mock<IToDoStore>();
    var handler = new UpdateToDoCommandHandler(toDoStore.Object);
    var toDoId = new ToDoId(Guid.NewGuid());
    var description = new Description("Updated description");

    _ = toDoStore
      .Setup(x => x.UpdateAsync(toDoId, description, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("boom"));

    // Act
    var result = await handler.Handle(new UpdateToDoCommand(toDoId, description), CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Equal("boom", result.Errors[0].Message);
  }
}
