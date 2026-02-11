using Apollo.Application.Reminders;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.Reminders;

public class CancelReminderCommandHandlerTests
{
  [Fact]
  public async Task HandleWithValidReminderCancelsSuccessfullyAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CancelReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var reminder = CreateReminder(reminderId, personId);
    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminder));

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(Enumerable.Empty<ToDoId>()));

    _ = reminderStore
      .Setup(x => x.DeleteAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await handler.Handle(
      new CancelReminderCommand(personId, reminderId),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    reminderStore.Verify(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()), Times.Once);
    reminderStore.Verify(x => x.DeleteAsync(reminderId, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWithNonExistentReminderReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CancelReminderCommandHandler(reminderStore.Object, scheduler.Object);

    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<Reminder>("Reminder not found"));

    // Act
    var result = await handler.Handle(
      new CancelReminderCommand(personId, reminderId),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Reminder not found", result.Errors[0].Message);
    reminderStore.Verify(x => x.DeleteAsync(It.IsAny<ReminderId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWithUnauthorizedPersonReturnsFailureAsync()
  {
    // Arrange
    var authorizedPersonId = new PersonId(Guid.NewGuid());
    var unauthorizedPersonId = new PersonId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CancelReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var reminder = CreateReminder(reminderId, authorizedPersonId);
    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminder));

    // Act
    var result = await handler.Handle(
      new CancelReminderCommand(unauthorizedPersonId, reminderId),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("permission", result.Errors[0].Message, StringComparison.OrdinalIgnoreCase);
    reminderStore.Verify(x => x.DeleteAsync(It.IsAny<ReminderId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWithReminderLinkedToToDosReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());
    var linkedToDoId = new ToDoId(Guid.NewGuid());
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CancelReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var reminder = CreateReminder(reminderId, personId);
    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminder));

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(new[] { linkedToDoId }.AsEnumerable()));

    // Act
    var result = await handler.Handle(
      new CancelReminderCommand(personId, reminderId),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("linked to a todo", result.Errors[0].Message, StringComparison.OrdinalIgnoreCase);
    reminderStore.Verify(x => x.DeleteAsync(It.IsAny<ReminderId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWithQuartzJobIdCleansUpJobWhenNoOtherRemindersUseItAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CancelReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var reminder = CreateReminder(reminderId, personId, quartzJobId);
    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminder));

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(Enumerable.Empty<ToDoId>()));

    _ = reminderStore
      .Setup(x => x.DeleteAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.GetByQuartzJobIdAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(Enumerable.Empty<Reminder>()));

    _ = scheduler
      .Setup(x => x.DeleteJobAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await handler.Handle(
      new CancelReminderCommand(personId, reminderId),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    scheduler.Verify(x => x.DeleteJobAsync(quartzJobId, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWithQuartzJobIdKeepsJobWhenOtherRemindersUseItAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var otherReminderId = new ReminderId(Guid.NewGuid());
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CancelReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var reminder = CreateReminder(reminderId, personId, quartzJobId);
    var otherReminder = CreateReminder(otherReminderId, personId, quartzJobId);

    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminder));

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(Enumerable.Empty<ToDoId>()));

    _ = reminderStore
      .Setup(x => x.DeleteAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.GetByQuartzJobIdAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(new[] { otherReminder }.AsEnumerable()));

    // Act
    var result = await handler.Handle(
      new CancelReminderCommand(personId, reminderId),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    scheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWithDeleteFailureReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CancelReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var reminder = CreateReminder(reminderId, personId);
    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminder));

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(Enumerable.Empty<ToDoId>()));

    _ = reminderStore
      .Setup(x => x.DeleteAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail("Database error"));

    // Act
    var result = await handler.Handle(
      new CancelReminderCommand(personId, reminderId),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Failed to delete reminder", result.Errors[0].Message);
  }

  [Fact]
  public async Task HandleWithExceptionReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CancelReminderCommandHandler(reminderStore.Object, scheduler.Object);

    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("Unexpected error"));

    // Act
    var result = await handler.Handle(
      new CancelReminderCommand(personId, reminderId),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Unexpected error", result.Errors[0].Message);
  }

  [Fact]
  public async Task HandleWithReminderWithoutQuartzJobIdStillDeletesSuccessfullyAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CancelReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var reminder = CreateReminder(reminderId, personId, null);
    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminder));

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(Enumerable.Empty<ToDoId>()));

    _ = reminderStore
      .Setup(x => x.DeleteAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await handler.Handle(
      new CancelReminderCommand(personId, reminderId),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    scheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
    reminderStore.Verify(x => x.DeleteAsync(reminderId, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWithLinkedToDoIdsQueryFailureStillContinuesAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CancelReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var reminder = CreateReminder(reminderId, personId);
    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminder));

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<IEnumerable<ToDoId>>("Query error"));

    _ = reminderStore
      .Setup(x => x.DeleteAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await handler.Handle(
      new CancelReminderCommand(personId, reminderId),
      CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    reminderStore.Verify(x => x.DeleteAsync(reminderId, It.IsAny<CancellationToken>()), Times.Once);
  }

  private static Reminder CreateReminder(ReminderId reminderId, PersonId personId, QuartzJobId? quartzJobId = null)
  {
    return new Reminder
    {
      Id = reminderId,
      PersonId = personId,
      Details = new Details("Test reminder"),
      ReminderTime = new ReminderTime(DateTime.UtcNow.AddHours(1)),
      QuartzJobId = quartzJobId,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
