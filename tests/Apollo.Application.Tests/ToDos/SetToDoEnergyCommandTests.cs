using Apollo.Application.ToDos;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.ToDos;

public class SetToDoEnergyCommandTests
{
  [Fact]
  public async Task HandleWithValidInputReturnsSuccessAsync()
  {
    // Arrange
    var toDoStore = new Mock<IToDoStore>();
    var handler = new SetToDoEnergyCommandHandler(toDoStore.Object);
    var personId = new PersonId(Guid.NewGuid());
    var toDoId = new ToDoId(Guid.NewGuid());
    var energy = new Energy(Level.Yellow);

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(new ToDo
      {
        Id = toDoId,
        PersonId = personId,
        Description = new Description("Test"),
        Priority = new Priority(Level.Green),
        Energy = new Energy(Level.Green),
        Interest = new Interest(Level.Green),
        CreatedOn = new Domain.Common.ValueObjects.CreatedOn(DateTime.UtcNow),
        UpdatedOn = new Domain.Common.ValueObjects.UpdatedOn(DateTime.UtcNow)
      }));

    _ = toDoStore
      .Setup(x => x.UpdateEnergyAsync(toDoId, energy, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await handler.Handle(
      new SetToDoEnergyCommand(personId, toDoId, energy),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    toDoStore.Verify(x => x.UpdateEnergyAsync(toDoId, energy, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWithWrongOwnerReturnsFailureAsync()
  {
    // Arrange
    var toDoStore = new Mock<IToDoStore>();
    var handler = new SetToDoEnergyCommandHandler(toDoStore.Object);
    var personId = new PersonId(Guid.NewGuid());
    var differentPersonId = new PersonId(Guid.NewGuid());
    var toDoId = new ToDoId(Guid.NewGuid());
    var energy = new Energy(Level.Yellow);

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(new ToDo
      {
        Id = toDoId,
        PersonId = differentPersonId, // Different owner
        Description = new Description("Test"),
        Priority = new Priority(Level.Green),
        Energy = new Energy(Level.Green),
        Interest = new Interest(Level.Green),
        CreatedOn = new Domain.Common.ValueObjects.CreatedOn(DateTime.UtcNow),
        UpdatedOn = new Domain.Common.ValueObjects.UpdatedOn(DateTime.UtcNow)
      }));

    // Act
    var result = await handler.Handle(
      new SetToDoEnergyCommand(personId, toDoId, energy),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("permission", result.Errors[0].Message.ToLowerInvariant());
    toDoStore.Verify(x => x.UpdateEnergyAsync(It.IsAny<ToDoId>(), It.IsAny<Energy>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWithNonexistentToDoReturnsFailureAsync()
  {
    // Arrange
    var toDoStore = new Mock<IToDoStore>();
    var handler = new SetToDoEnergyCommandHandler(toDoStore.Object);
    var personId = new PersonId(Guid.NewGuid());
    var toDoId = new ToDoId(Guid.NewGuid());
    var energy = new Energy(Level.Yellow);

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<ToDo>("To-Do not found"));

    // Act
    var result = await handler.Handle(
      new SetToDoEnergyCommand(personId, toDoId, energy),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("not found", result.Errors[0].Message.ToLowerInvariant());
    toDoStore.Verify(x => x.UpdateEnergyAsync(It.IsAny<ToDoId>(), It.IsAny<Energy>(), It.IsAny<CancellationToken>()), Times.Never);
  }
}
