using Apollo.Application.ToDos;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.ToDos.Handlers;

public class RemoveReminderCommandHandlerTests
{
  [Fact]
  public async Task HandleUnlinksReminderAndDeletesWhenNoLinksRemainAsync()
  {
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new RemoveReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var reminder = CreateReminder(reminderId, quartzJobId);

    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminder));

    _ = reminderStore
      .Setup(x => x.UnlinkFromToDoAsync(reminderId, toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDoId>>([]));

    _ = scheduler
      .Setup(x => x.DeleteJobAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.DeleteAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    var result = await handler.Handle(new RemoveReminderCommand(toDoId, reminderId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    reminderStore.Verify(x => x.UnlinkFromToDoAsync(reminderId, toDoId, It.IsAny<CancellationToken>()), Times.Once);
    scheduler.Verify(x => x.DeleteJobAsync(quartzJobId, It.IsAny<CancellationToken>()), Times.Once);
    reminderStore.Verify(x => x.DeleteAsync(reminderId, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleUnlinksReminderButDoesNotDeleteWhenOtherLinksExistAsync()
  {
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new RemoveReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var otherToDoId = new ToDoId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var reminder = CreateReminder(reminderId, quartzJobId);

    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminder));

    _ = reminderStore
      .Setup(x => x.UnlinkFromToDoAsync(reminderId, toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDoId>>([otherToDoId]));

    var result = await handler.Handle(new RemoveReminderCommand(toDoId, reminderId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    reminderStore.Verify(x => x.UnlinkFromToDoAsync(reminderId, toDoId, It.IsAny<CancellationToken>()), Times.Once);
    scheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
    reminderStore.Verify(x => x.DeleteAsync(It.IsAny<ReminderId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWhenReminderNotFoundReturnsFailAsync()
  {
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new RemoveReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var reminderId = new ReminderId(Guid.NewGuid());

    _ = reminderStore
      .Setup(x => x.GetAsync(reminderId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<Reminder>("Reminder not found"));

    var result = await handler.Handle(new RemoveReminderCommand(toDoId, reminderId), CancellationToken.None);

    Assert.True(result.IsFailed);
    reminderStore.Verify(x => x.UnlinkFromToDoAsync(It.IsAny<ReminderId>(), It.IsAny<ToDoId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  private static Reminder CreateReminder(ReminderId reminderId, QuartzJobId quartzJobId)
  {
    return new Reminder
    {
      Id = reminderId,
      Details = new Details("test"),
      ReminderTime = new ReminderTime(DateTime.UtcNow.AddMinutes(30)),
      QuartzJobId = quartzJobId,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
