using Apollo.Application.ToDos.Commands;
using Apollo.Application.ToDos.Handlers;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.ToDos.Handlers;

public class CreateToDoCommandHandlerTests
{
  [Fact]
  public async Task HandleWithReminderDateSchedulesJobAndCreatesReminderAsync()
  {
    var store = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CreateToDoCommandHandler(store.Object, reminderStore.Object, scheduler.Object);

    var personId = new PersonId(Guid.NewGuid());
    var description = new Description("test");
    var reminderDate = DateTime.UtcNow.AddMinutes(5);
    var quartzJobId = new QuartzJobId(Guid.NewGuid());

    var sequence = new MockSequence();

    _ = store
      .InSequence(sequence)
      .Setup(x => x.CreateAsync(
        It.IsAny<ToDoId>(),
        personId,
        description,
        It.IsAny<Priority>(),
        It.IsAny<Energy>(),
        It.IsAny<Interest>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync((ToDoId id, PersonId pId, Description desc, Priority priority, Energy energy, Interest interest, CancellationToken _) => Result.Ok(new ToDo
      {
        CreatedOn = new CreatedOn(DateTime.UtcNow),
        Description = desc,
        Energy = energy,
        Id = id,
        Interest = interest,
        PersonId = pId,
        Priority = priority,
        UpdatedOn = new UpdatedOn(DateTime.UtcNow)
      }));

    _ = scheduler
      .InSequence(sequence)
      .Setup(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(quartzJobId));

    _ = reminderStore
      .InSequence(sequence)
      .Setup(x => x.CreateAsync(It.IsAny<ReminderId>(), personId, It.IsAny<Details>(), It.IsAny<ReminderTime>(), quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((ReminderId id, PersonId pid, Details details, ReminderTime time, QuartzJobId jobId, CancellationToken _) => Result.Ok(new Reminder
      {
        Id = id,
        PersonId = pid,
        Details = details,
        ReminderTime = time,
        QuartzJobId = jobId,
        CreatedOn = new CreatedOn(DateTime.UtcNow),
        UpdatedOn = new UpdatedOn(DateTime.UtcNow)
      }));

    _ = reminderStore
      .InSequence(sequence)
      .Setup(x => x.LinkToToDoAsync(It.IsAny<ReminderId>(), It.IsAny<ToDoId>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = scheduler
      .InSequence(sequence)
      .Setup(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(quartzJobId));

    var result = await handler.Handle(new CreateToDoCommand(personId, description, reminderDate), CancellationToken.None);

    Assert.True(result.IsSuccess);
    scheduler.Verify(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()), Times.Exactly(2));
    reminderStore.Verify(x => x.CreateAsync(It.IsAny<ReminderId>(), It.IsAny<PersonId>(), It.IsAny<Details>(), It.IsAny<ReminderTime>(), quartzJobId, It.IsAny<CancellationToken>()), Times.Once);
    reminderStore.Verify(x => x.LinkToToDoAsync(It.IsAny<ReminderId>(), It.IsAny<ToDoId>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWithoutReminderDateDoesNotCreateReminderAsync()
  {
    var store = new Mock<IToDoStore>();
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CreateToDoCommandHandler(store.Object, reminderStore.Object, scheduler.Object);

    var personId = new PersonId(Guid.NewGuid());
    var description = new Description("test");

    _ = store
      .Setup(x => x.CreateAsync(
        It.IsAny<ToDoId>(),
        personId,
        description,
        It.IsAny<Priority>(),
        It.IsAny<Energy>(),
        It.IsAny<Interest>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync((ToDoId id, PersonId pId, Description desc, Priority priority, Energy energy, Interest interest, CancellationToken _) => Result.Ok(new ToDo
      {
        CreatedOn = new CreatedOn(DateTime.UtcNow),
        Description = desc,
        Energy = energy,
        Id = id,
        Interest = interest,
        PersonId = pId,
        Priority = priority,
        UpdatedOn = new UpdatedOn(DateTime.UtcNow)
      }));

    var result = await handler.Handle(new CreateToDoCommand(personId, description), CancellationToken.None);

    Assert.True(result.IsSuccess);
    scheduler.Verify(x => x.GetOrCreateJobAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    reminderStore.Verify(x => x.CreateAsync(It.IsAny<ReminderId>(), It.IsAny<PersonId>(), It.IsAny<Details>(), It.IsAny<ReminderTime>(), It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }
}
