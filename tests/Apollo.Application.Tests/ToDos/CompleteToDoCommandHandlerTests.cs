using Apollo.Application.ToDos;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.ToDos;

public class CompleteToDoCommandHandlerTests
{
  [Fact]
  public async Task HandleWhenCompleteFailsReturnsFailAndDoesNotDeleteJob()
  {
    var toDoStore = new Mock<IToDoStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, toDoReminderScheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreateToDo(toDoId, quartzJobId)));

    var fail = Result.Fail("fail");
    _ = toDoStore
      .Setup(x => x.CompleteAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(fail);

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsFailed);
    Assert.Equal("fail", result.Errors[0].Message);
    toDoStore.Verify(x => x.GetToDosByQuartzJobIdAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
    toDoReminderScheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWhenGetFailsStillCompletesAndDoesNotDeleteJob()
  {
    var toDoStore = new Mock<IToDoStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, toDoReminderScheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<ToDo>("nope"));

    _ = toDoStore
      .Setup(x => x.CompleteAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    toDoStore.Verify(x => x.CompleteAsync(toDoId, It.IsAny<CancellationToken>()), Times.Once);
    toDoStore.Verify(x => x.GetToDosByQuartzJobIdAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
    toDoReminderScheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWhenNoReminderDoesNotDeleteJob()
  {
    var toDoStore = new Mock<IToDoStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, toDoReminderScheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreateToDo(toDoId)));

    _ = toDoStore
      .Setup(x => x.CompleteAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    toDoStore.Verify(x => x.GetToDosByQuartzJobIdAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
    toDoReminderScheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWhenReminderAndNoRemainingToDosDeletesJob()
  {
    var toDoStore = new Mock<IToDoStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, toDoReminderScheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreateToDo(toDoId, quartzJobId)));

    _ = toDoStore
      .Setup(x => x.CompleteAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = toDoStore
      .SetupSequence(x => x.GetToDosByQuartzJobIdAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDo>>([]))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDo>>([]));

    _ = toDoReminderScheduler
      .Setup(x => x.DeleteJobAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    toDoReminderScheduler.Verify(x => x.DeleteJobAsync(quartzJobId, It.IsAny<CancellationToken>()), Times.Once);
    toDoReminderScheduler.Verify(x => x.GetOrCreateJobAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWhenReminderAndNoRemainingThenRemainingAppearsRecreatesJob()
  {
    var toDoStore = new Mock<IToDoStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, toDoReminderScheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var reminderDate = new DateTime(2030, 01, 01, 12, 00, 00, DateTimeKind.Utc);

    var remainingToDo = CreateToDo(new ToDoId(Guid.NewGuid()), quartzJobId) with
    {
      Reminders =
      [
        new Reminder
        {
          AcknowledgedOn = null,
          CreatedOn = new CreatedOn(DateTime.UtcNow),
          Details = new Details("test"),
          Id = new ReminderId(Guid.NewGuid()),
          QuartzJobId = quartzJobId,
          ReminderTime = new ReminderTime(reminderDate),
          UpdatedOn = new UpdatedOn(DateTime.UtcNow)
        }
      ]
    };

    var sequence = new MockSequence();

    _ = toDoStore
      .InSequence(sequence)
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreateToDo(toDoId, quartzJobId)));

    _ = toDoStore
      .InSequence(sequence)
      .Setup(x => x.CompleteAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = toDoStore
      .InSequence(sequence)
      .Setup(x => x.GetToDosByQuartzJobIdAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDo>>([]));

    _ = toDoReminderScheduler
      .InSequence(sequence)
      .Setup(x => x.DeleteJobAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = toDoStore
      .InSequence(sequence)
      .Setup(x => x.GetToDosByQuartzJobIdAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDo>>([remainingToDo]));

    _ = toDoReminderScheduler
      .InSequence(sequence)
      .Setup(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(quartzJobId));

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    toDoReminderScheduler.Verify(x => x.DeleteJobAsync(quartzJobId, It.IsAny<CancellationToken>()), Times.Once);
    toDoReminderScheduler.Verify(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWhenReminderAndRemainingToDosExistDoesNotDeleteJob()
  {
    var toDoStore = new Mock<IToDoStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, toDoReminderScheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var quartzJobId = new QuartzJobId(Guid.NewGuid());

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreateToDo(toDoId, quartzJobId)));

    _ = toDoStore
      .Setup(x => x.CompleteAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = toDoStore
      .Setup(x => x.GetToDosByQuartzJobIdAsync(quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDo>>([CreateToDo(new ToDoId(Guid.NewGuid()), quartzJobId)]));

    var result = await handler.Handle(new CompleteToDoCommand(toDoId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    toDoReminderScheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWhenExceptionThrownReturnsFail()
  {
    var toDoStore = new Mock<IToDoStore>();
    var toDoReminderScheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CompleteToDoCommandHandler(toDoStore.Object, toDoReminderScheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
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

  private static ToDo CreateToDo(ToDoId toDoId, QuartzJobId? quartzJobId = null)
  {
    return new ToDo
    {
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      Description = new Description("test"),
      Energy = new Energy(0),
      Id = toDoId,
      Interest = new Interest(0),
      PersonId = new PersonId(Guid.NewGuid()),
      Priority = new Priority(0),
      Reminders = quartzJobId is null ? [] : [CreateReminder(quartzJobId)],
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
