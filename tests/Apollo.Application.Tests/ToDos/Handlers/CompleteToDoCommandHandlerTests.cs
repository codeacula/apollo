using Apollo.Application.ToDos.Commands;
using Apollo.Application.ToDos.Handlers;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Microsoft.Extensions.Logging;

using Moq;

namespace Apollo.Application.Tests.ToDos.Handlers;

public class CompleteToDoCommandHandlerTests
{
  [Fact]
  public async Task HandleWhenCompleteFailsReturnsFailAndDoesNotDeleteJobAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var logger = new Mock<ILogger<CompleteToDoCommandHandler>>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, reminderStore.Object, toDoReminderScheduler.Object, logger.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var reminder = CreateReminder(quartzJobId);

    _ = reminderStore
      .Setup(x => x.GetByToDoIdAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<Reminder>>([reminder]));

    var fail = Result.Fail("fail");
    _ = toDoStore
      .Setup(x => x.CompleteAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(fail);

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsFailed);
    Assert.Equal("fail", result.Errors[0].Message);
    reminderStore.Verify(x => x.UnlinkFromToDoAsync(It.IsAny<ReminderId>(), It.IsAny<ToDoId>(), It.IsAny<CancellationToken>()), Times.Never);
    toDoReminderScheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWhenNoRemindersDoesNotDeleteJobAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var logger = new Mock<ILogger<CompleteToDoCommandHandler>>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, reminderStore.Object, toDoReminderScheduler.Object, logger.Object);

    var toDoId = new ToDoId(Guid.NewGuid());

    _ = reminderStore
      .Setup(x => x.GetByToDoIdAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<Reminder>>([]));

    _ = toDoStore
      .Setup(x => x.CompleteAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    reminderStore.Verify(x => x.UnlinkFromToDoAsync(It.IsAny<ReminderId>(), It.IsAny<ToDoId>(), It.IsAny<CancellationToken>()), Times.Never);
    toDoReminderScheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWhenReminderAndNoRemainingLinksDeletesJobAndReminderAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var logger = new Mock<ILogger<CompleteToDoCommandHandler>>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, reminderStore.Object, toDoReminderScheduler.Object, logger.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var reminder = CreateReminder(quartzJobId);

    _ = reminderStore
      .Setup(x => x.GetByToDoIdAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<Reminder>>([reminder]));

    _ = toDoStore
      .Setup(x => x.CompleteAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.UnlinkFromToDoAsync(reminder.Id, toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminder.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDoId>>([]));

    _ = toDoReminderScheduler
      .Setup(x => x.DeleteJobAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.DeleteAsync(reminder.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    reminderStore.Verify(x => x.UnlinkFromToDoAsync(reminder.Id, toDoId, It.IsAny<CancellationToken>()), Times.Once);
    toDoReminderScheduler.Verify(x => x.DeleteJobAsync(quartzJobId, It.IsAny<CancellationToken>()), Times.Once);
    reminderStore.Verify(x => x.DeleteAsync(reminder.Id, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWhenReminderAndRemainingLinksExistDoesNotDeleteJobAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var logger = new Mock<ILogger<CompleteToDoCommandHandler>>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, reminderStore.Object, toDoReminderScheduler.Object, logger.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var otherToDoId = new ToDoId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var reminder = CreateReminder(quartzJobId);

    _ = reminderStore
      .Setup(x => x.GetByToDoIdAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<Reminder>>([reminder]));

    _ = toDoStore
      .Setup(x => x.CompleteAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.UnlinkFromToDoAsync(reminder.Id, toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminder.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDoId>>([otherToDoId]));

    _ = toDoReminderScheduler
      .Setup(x => x.GetOrCreateJobAsync(reminder.ReminderTime.Value, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(quartzJobId));

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    reminderStore.Verify(x => x.UnlinkFromToDoAsync(reminder.Id, toDoId, It.IsAny<CancellationToken>()), Times.Once);
    toDoReminderScheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
    reminderStore.Verify(x => x.DeleteAsync(It.IsAny<ReminderId>(), It.IsAny<CancellationToken>()), Times.Never);
    toDoReminderScheduler.Verify(x => x.GetOrCreateJobAsync(reminder.ReminderTime.Value, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWhenExceptionThrownReturnsFailAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var logger = new Mock<ILogger<CompleteToDoCommandHandler>>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, reminderStore.Object, toDoReminderScheduler.Object, logger.Object);

    var toDoId = new ToDoId(Guid.NewGuid());

    _ = reminderStore
      .Setup(x => x.GetByToDoIdAsync(toDoId, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("boom"));

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsFailed);
    Assert.Equal("boom", result.Errors[0].Message);
  }

  private static Reminder CreateReminder(QuartzJobId? quartzJobId)
  {
    return new Reminder
    {
      AcknowledgedOn = null,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      Details = new Details("test"),
      Id = new ReminderId(Guid.NewGuid()),
      QuartzJobId = quartzJobId,
      ReminderTime = new ReminderTime(DateTime.UtcNow.AddMinutes(5)),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
