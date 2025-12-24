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

public class CreateToDoCommandHandlerTests
{
  [Fact]
  public async Task HandleWithReminderDateSchedulesJobAndPersistsJobIdAsync()
  {
    var store = new Mock<IToDoStore>();
    var scheduler = new Mock<IToDoReminderScheduler>();
    var handler = new CreateToDoCommandHandler(store.Object, scheduler.Object);

    var personId = new PersonId(Guid.NewGuid());
    var description = new Description("test");
    var reminderDate = DateTime.UtcNow.AddMinutes(5);
    var quartzJobId = new QuartzJobId(Guid.NewGuid());

    var sequence = new MockSequence();

    _ = store
      .InSequence(sequence)
      .Setup(x => x.CreateAsync(It.IsAny<ToDoId>(), personId, description, It.IsAny<CancellationToken>()))
      .ReturnsAsync((ToDoId id, PersonId pId, Description desc, CancellationToken _) => Result.Ok(new ToDo
      {
        CreatedOn = new CreatedOn(DateTime.UtcNow),
        Description = desc,
        Energy = new Energy(0),
        Id = id,
        Interest = new Interest(0),
        PersonId = pId,
        Priority = new Priority(0),
        UpdatedOn = new UpdatedOn(DateTime.UtcNow)
      }));

    _ = scheduler
      .InSequence(sequence)
      .Setup(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(quartzJobId));

    _ = store
      .InSequence(sequence)
      .Setup(x => x.SetReminderAsync(It.IsAny<ToDoId>(), reminderDate, quartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = scheduler
      .InSequence(sequence)
      .Setup(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(quartzJobId));

    var result = await handler.Handle(new CreateToDoCommand(personId, description, reminderDate), CancellationToken.None);

    Assert.True(result.IsSuccess);
    scheduler.Verify(x => x.GetOrCreateJobAsync(reminderDate, It.IsAny<CancellationToken>()), Times.Exactly(2));
    store.Verify(x => x.SetReminderAsync(It.IsAny<ToDoId>(), reminderDate, quartzJobId, It.IsAny<CancellationToken>()), Times.Once);
  }
}
