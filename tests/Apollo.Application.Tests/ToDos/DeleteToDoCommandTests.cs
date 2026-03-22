using Apollo.Application.Tests.TestSupport;
using Apollo.Application.ToDos;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Microsoft.Extensions.Logging;

using Moq;

namespace Apollo.Application.Tests.ToDos;

public class DeleteToDoCommandTests
{
  [Fact]
  public async Task HandleWhenUnlinkFailsLogsWarningButContinuesAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var logger = new Mock<ILogger<DeleteToDoCommandHandler>>();
    _ = logger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
    var handler = new DeleteToDoCommandHandler(toDoStore.Object, reminderStore.Object, toDoReminderScheduler.Object, logger.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var reminder = ApplicationTestData.CreateReminder(quartzJobId: quartzJobId);

    _ = reminderStore
      .Setup(x => x.GetByToDoIdAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<Reminder>>([reminder]));

    _ = toDoStore
      .Setup(x => x.DeleteAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.UnlinkFromToDoAsync(reminder.Id, toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail("unlink failed"));

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminder.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDoId>>([]));

    _ = toDoReminderScheduler
      .Setup(x => x.DeleteJobAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.DeleteAsync(reminder.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    var result = await handler.Handle(new DeleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    logger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Failed to unlink reminder")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task HandleWhenDeleteJobFailsLogsWarningButContinuesAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var logger = new Mock<ILogger<DeleteToDoCommandHandler>>();
    _ = logger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
    var handler = new DeleteToDoCommandHandler(toDoStore.Object, reminderStore.Object, toDoReminderScheduler.Object, logger.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var reminder = ApplicationTestData.CreateReminder(quartzJobId: quartzJobId);

    _ = reminderStore
      .Setup(x => x.GetByToDoIdAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<Reminder>>([reminder]));

    _ = toDoStore
      .Setup(x => x.DeleteAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.UnlinkFromToDoAsync(reminder.Id, toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = reminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminder.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDoId>>([]));

    _ = toDoReminderScheduler
      .Setup(x => x.DeleteJobAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail("delete job failed"));

    _ = reminderStore
      .Setup(x => x.DeleteAsync(reminder.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    var result = await handler.Handle(new DeleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    logger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Failed to delete reminder job")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task HandleWhenDeleteReminderFailsLogsWarningButContinuesAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var logger = new Mock<ILogger<DeleteToDoCommandHandler>>();
    _ = logger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
    var handler = new DeleteToDoCommandHandler(toDoStore.Object, reminderStore.Object, toDoReminderScheduler.Object, logger.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var reminder = ApplicationTestData.CreateReminder(quartzJobId: quartzJobId);

    _ = reminderStore
      .Setup(x => x.GetByToDoIdAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<Reminder>>([reminder]));

    _ = toDoStore
      .Setup(x => x.DeleteAsync(toDoId, It.IsAny<CancellationToken>()))
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

    var result = await handler.Handle(new DeleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    logger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Failed to delete reminder")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }
}
