using Apollo.Application.Tests.TestSupport;
using Apollo.Application.ToDos;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Microsoft.Extensions.Logging;

using Moq;

namespace Apollo.Application.Tests.ToDos;

public class CompleteToDoCommandTests
{
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

  [Fact]
  public async Task HandleWhenDeleteReminderFailsReturnsFailureAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var logger = new Mock<ILogger<CompleteToDoCommandHandler>>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, reminderStore.Object, toDoReminderScheduler.Object, logger.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var reminder = ApplicationTestData.CreateReminder(quartzJobId: quartzJobId);

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
      .ReturnsAsync(Result.Fail("delete reminder failed"));

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsFailed);
    Assert.Equal("delete reminder failed", result.Errors[0].Message);
  }

  [Fact]
  public async Task HandleWhenRecreateJobFailsReturnsFailureAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var logger = new Mock<ILogger<CompleteToDoCommandHandler>>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, reminderStore.Object, toDoReminderScheduler.Object, logger.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var otherToDoId = new ToDoId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var reminder = ApplicationTestData.CreateReminder(quartzJobId: quartzJobId);

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
      .ReturnsAsync(Result.Fail<QuartzJobId>("job failed"));

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsFailed);
    Assert.Equal("Failed to ensure reminder job still exists.", result.Errors[0].Message);
  }
}
