using Apollo.Application.ToDos.Commands;
using Apollo.Application.ToDos.Handlers;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.ToDos.Handlers;

public class AddReminderCommandHandlerTests
{
  [Fact]
  public async Task HandleCreatesReminderAndLinksToToDoAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new AddReminderCommandHandler(toDoStore.Object, reminderStore.Object, scheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var reminderDate = DateTime.UtcNow.AddMinutes(30);
    var quartzJobId = new QuartzJobId(Guid.NewGuid());
    var toDo = CreateToDo(toDoId);

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(toDo));

    _ = scheduler
      .Setup(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(quartzJobId));

    _ = reminderStore
      .Setup(x => x.CreateAsync(It.IsAny<ReminderId>(), It.IsAny<PersonId>(), It.IsAny<Details>(), It.IsAny<ReminderTime>(), quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((ReminderId id, PersonId personId, Details details, ReminderTime time, QuartzJobId jobId, CancellationToken _) => Result.Ok(new Reminder
      {
        Id = id,
        PersonId = personId,
        Details = details,
        ReminderTime = time,
        QuartzJobId = jobId,
        CreatedOn = new CreatedOn(DateTime.UtcNow),
        UpdatedOn = new UpdatedOn(DateTime.UtcNow)
      }));

    _ = reminderStore
      .Setup(x => x.LinkToToDoAsync(It.IsAny<ReminderId>(), toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    var result = await handler.Handle(new AddReminderCommand(toDoId, reminderDate), CancellationToken.None);

    Assert.True(result.IsSuccess);
    reminderStore.Verify(x => x.CreateAsync(It.IsAny<ReminderId>(), It.IsAny<PersonId>(), It.IsAny<Details>(), It.IsAny<ReminderTime>(), quartzJobId, It.IsAny<CancellationToken>()), Times.Once);
    reminderStore.Verify(x => x.LinkToToDoAsync(It.IsAny<ReminderId>(), toDoId, It.IsAny<CancellationToken>()), Times.Once);
    scheduler.Verify(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()), Times.Exactly(2));
  }

  [Fact]
  public async Task HandleWhenToDoNotFoundReturnsFailAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new AddReminderCommandHandler(toDoStore.Object, reminderStore.Object, scheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var reminderDate = DateTime.UtcNow.AddMinutes(30);

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<ToDo>("ToDo not found"));

    var result = await handler.Handle(new AddReminderCommand(toDoId, reminderDate), CancellationToken.None);

    Assert.True(result.IsFailed);
    scheduler.Verify(x => x.GetOrCreateJobAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    reminderStore.Verify(x => x.CreateAsync(It.IsAny<ReminderId>(), It.IsAny<PersonId>(), It.IsAny<Details>(), It.IsAny<ReminderTime>(), It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWhenSchedulerFailsReturnsFailAsync()
  {
    var toDoStore = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new AddReminderCommandHandler(toDoStore.Object, reminderStore.Object, scheduler.Object);

    var toDoId = new ToDoId(Guid.NewGuid());
    var reminderDate = DateTime.UtcNow.AddMinutes(30);
    var toDo = CreateToDo(toDoId);

    _ = toDoStore
      .Setup(x => x.GetAsync(toDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(toDo));

    _ = scheduler
      .Setup(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<QuartzJobId>("Scheduler failed"));

    var result = await handler.Handle(new AddReminderCommand(toDoId, reminderDate), CancellationToken.None);

    Assert.True(result.IsFailed);
    reminderStore.Verify(x => x.CreateAsync(It.IsAny<ReminderId>(), It.IsAny<PersonId>(), It.IsAny<Details>(), It.IsAny<ReminderTime>(), It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  private static ToDo CreateToDo(ToDoId toDoId)
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
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
