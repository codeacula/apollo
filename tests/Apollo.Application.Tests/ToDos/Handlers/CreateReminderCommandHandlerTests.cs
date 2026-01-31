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

public class CreateReminderCommandHandlerTests
{
  [Fact]
  public async Task HandleCreatesStandaloneReminderAsync()
  {
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CreateReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var personId = new PersonId(Guid.NewGuid());
    const string details = "Get coffee";
    var reminderDate = DateTime.UtcNow.AddMinutes(30);
    var quartzJobId = new QuartzJobId(Guid.NewGuid());

    _ = scheduler
      .Setup(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(quartzJobId));

    _ = reminderStore
      .Setup(x => x.CreateAsync(It.IsAny<ReminderId>(), personId, It.IsAny<Details>(), It.IsAny<ReminderTime>(), quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((ReminderId id, PersonId pid, Details det, ReminderTime time, QuartzJobId jobId, CancellationToken _) => Result.Ok(new Reminder
      {
        Id = id,
        PersonId = pid,
        Details = det,
        ReminderTime = time,
        QuartzJobId = jobId,
        CreatedOn = new CreatedOn(DateTime.UtcNow),
        UpdatedOn = new UpdatedOn(DateTime.UtcNow)
      }));

    var result = await handler.Handle(new CreateReminderCommand(personId, details, reminderDate), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(personId, result.Value.PersonId);
    Assert.Equal(details, result.Value.Details.Value);
    scheduler.Verify(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()), Times.Exactly(2));
    reminderStore.Verify(x => x.CreateAsync(It.IsAny<ReminderId>(), personId, It.IsAny<Details>(), It.IsAny<ReminderTime>(), quartzJobId, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task HandleWhenSchedulerFailsReturnsFailAsync()
  {
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CreateReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var personId = new PersonId(Guid.NewGuid());
    const string details = "Get coffee";
    var reminderDate = DateTime.UtcNow.AddMinutes(30);

    _ = scheduler
      .Setup(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<QuartzJobId>("Scheduler failed"));

    var result = await handler.Handle(new CreateReminderCommand(personId, details, reminderDate), CancellationToken.None);

    Assert.True(result.IsFailed);
    reminderStore.Verify(x => x.CreateAsync(It.IsAny<ReminderId>(), It.IsAny<PersonId>(), It.IsAny<Details>(), It.IsAny<ReminderTime>(), It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task HandleWhenReminderStoreFailsReturnsFailAsync()
  {
    var reminderStore = new Mock<IReminderStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CreateReminderCommandHandler(reminderStore.Object, scheduler.Object);

    var personId = new PersonId(Guid.NewGuid());
    const string details = "Get coffee";
    var reminderDate = DateTime.UtcNow.AddMinutes(30);
    var quartzJobId = new QuartzJobId(Guid.NewGuid());

    _ = scheduler
      .Setup(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(quartzJobId));

    _ = reminderStore
      .Setup(x => x.CreateAsync(It.IsAny<ReminderId>(), personId, It.IsAny<Details>(), It.IsAny<ReminderTime>(), quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<Reminder>("Reminder creation failed"));

    var result = await handler.Handle(new CreateReminderCommand(personId, details, reminderDate), CancellationToken.None);

    Assert.True(result.IsFailed);
    scheduler.Verify(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()), Times.Once);
    reminderStore.Verify(x => x.CreateAsync(It.IsAny<ReminderId>(), personId, It.IsAny<Details>(), It.IsAny<ReminderTime>(), quartzJobId, It.IsAny<CancellationToken>()), Times.Once);
  }
}
