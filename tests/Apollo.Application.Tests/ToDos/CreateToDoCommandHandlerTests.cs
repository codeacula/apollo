using Apollo.Application.ToDos;
using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.ToDos;

public class CreateToDoCommandHandlerTests
{
  private readonly Mock<IToDoStore> _mockToDoStore;
  private readonly Mock<IToDoReminderScheduler> _mockReminderScheduler;
  private readonly CreateToDoCommandHandler _handler;

  public CreateToDoCommandHandlerTests()
  {
    _mockToDoStore = new Mock<IToDoStore>();
    _mockReminderScheduler = new Mock<IToDoReminderScheduler>();
    _handler = new CreateToDoCommandHandler(_mockToDoStore.Object, _mockReminderScheduler.Object);
  }

  [Fact]
  public async Task HandleWithoutReminderCreatesToDoSuccessfully()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var description = new Description("Test ToDo");
    var command = new CreateToDoCommand(personId, description, null);

    var expectedToDo = new ToDo
    {
      Id = new ToDoId(Guid.NewGuid()),
      PersonId = personId,
      Description = description,
      Priority = new Priority(0),
      Energy = new Energy(0),
      Interest = new Interest(0),
      CreatedOn = new(DateTime.UtcNow),
      UpdatedOn = new(DateTime.UtcNow)
    };

    _ = _mockToDoStore
      .Setup(x => x.CreateAsync(It.IsAny<ToDoId>(), personId, description, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(expectedToDo));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedToDo.Id, result.Value.Id);
    _mockReminderScheduler.Verify(
      x => x.ScheduleReminderAsync(It.IsAny<ToDoId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task HandleWithReminderSchedulesJobAndSetReminder()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var description = new Description("Test ToDo with reminder");
    var reminderDate = DateTime.UtcNow.AddDays(1);
    var command = new CreateToDoCommand(personId, description, reminderDate);

    var expectedToDo = new ToDo
    {
      Id = new ToDoId(Guid.NewGuid()),
      PersonId = personId,
      Description = description,
      Priority = new Priority(0),
      Energy = new Energy(0),
      Interest = new Interest(0),
      CreatedOn = new(DateTime.UtcNow),
      UpdatedOn = new(DateTime.UtcNow)
    };

    var quartzJobId = new QuartzJobId(Guid.NewGuid());

    _ = _mockToDoStore
      .Setup(x => x.CreateAsync(It.IsAny<ToDoId>(), personId, description, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(expectedToDo));

    _ = _mockReminderScheduler
      .Setup(x => x.ScheduleReminderAsync(It.IsAny<ToDoId>(), reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(quartzJobId));

    _ = _mockToDoStore
      .Setup(x => x.SetReminderAsync(It.IsAny<ToDoId>(), reminderDate, quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    _mockReminderScheduler.Verify(
      x => x.ScheduleReminderAsync(It.IsAny<ToDoId>(), reminderDate, It.IsAny<CancellationToken>()),
      Times.Once);
    _mockToDoStore.Verify(
      x => x.SetReminderAsync(It.IsAny<ToDoId>(), reminderDate, quartzJobId, It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task HandleWhenSchedulingFailsReturnsFailure()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var description = new Description("Test ToDo");
    var reminderDate = DateTime.UtcNow.AddDays(1);
    var command = new CreateToDoCommand(personId, description, reminderDate);

    var expectedToDo = new ToDo
    {
      Id = new ToDoId(Guid.NewGuid()),
      PersonId = personId,
      Description = description,
      Priority = new Priority(0),
      Energy = new Energy(0),
      Interest = new Interest(0),
      CreatedOn = new(DateTime.UtcNow),
      UpdatedOn = new(DateTime.UtcNow)
    };

    _ = _mockToDoStore
      .Setup(x => x.CreateAsync(It.IsAny<ToDoId>(), personId, description, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(expectedToDo));

    _ = _mockReminderScheduler
      .Setup(x => x.ScheduleReminderAsync(It.IsAny<ToDoId>(), reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<QuartzJobId>("Failed to schedule job"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("failed to schedule reminder", result.Errors[0].Message);
  }

  [Fact]
  public async Task HandleWhenSetReminderFailsCancelsScheduledJob()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var description = new Description("Test ToDo");
    var reminderDate = DateTime.UtcNow.AddDays(1);
    var command = new CreateToDoCommand(personId, description, reminderDate);

    var expectedToDo = new ToDo
    {
      Id = new ToDoId(Guid.NewGuid()),
      PersonId = personId,
      Description = description,
      Priority = new Priority(0),
      Energy = new Energy(0),
      Interest = new Interest(0),
      CreatedOn = new(DateTime.UtcNow),
      UpdatedOn = new(DateTime.UtcNow)
    };

    var quartzJobId = new QuartzJobId(Guid.NewGuid());

    _ = _mockToDoStore
      .Setup(x => x.CreateAsync(It.IsAny<ToDoId>(), personId, description, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(expectedToDo));

    _ = _mockReminderScheduler
      .Setup(x => x.ScheduleReminderAsync(It.IsAny<ToDoId>(), reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(quartzJobId));

    _ = _mockToDoStore
      .Setup(x => x.SetReminderAsync(It.IsAny<ToDoId>(), reminderDate, quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail("Database error"));

    _ = _mockReminderScheduler
      .Setup(x => x.CancelReminderAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("failed to set reminder", result.Errors[0].Message);
    _mockReminderScheduler.Verify(
      x => x.CancelReminderAsync(quartzJobId, It.IsAny<CancellationToken>()),
      Times.Once);
  }
}
