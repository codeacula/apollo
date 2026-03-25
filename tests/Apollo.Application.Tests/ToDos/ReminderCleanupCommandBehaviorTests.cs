using Apollo.Application.Tests.TestSupport;
using Apollo.Application.ToDos;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using MediatR;

using Microsoft.Extensions.Logging;

using Moq;

namespace Apollo.Application.Tests.ToDos;

public sealed class ReminderCleanupCommandBehaviorTests
{
  public static TheoryData<string> HandlerKinds => new()
  {
    "delete",
    "complete"
  };

  [Theory]
  [MemberData(nameof(HandlerKinds))]
  public async Task HandleWhenPrimaryActionFailsSkipsReminderCleanupAsync(string handlerKind)
  {
    var context = CreateContext(handlerKind);
    var reminder = ApplicationTestData.CreateReminder(quartzJobId: context.QuartzJobId);

    _ = context.ReminderStore
      .Setup(x => x.GetByToDoIdAsync(context.ToDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<Reminder>>([reminder]));

    SetupPrimaryAction(context, Result.Fail("fail"));

    var result = await ExecuteAsync(context);

    Assert.True(result.IsFailed);
    Assert.Equal("fail", result.Errors[0].Message);
    context.ReminderStore.Verify(x => x.UnlinkFromToDoAsync(It.IsAny<ReminderId>(), It.IsAny<ToDoId>(), It.IsAny<CancellationToken>()), Times.Never);
    context.Scheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Theory]
  [MemberData(nameof(HandlerKinds))]
  public async Task HandleWhenNoRemindersSkipsCleanupAsync(string handlerKind)
  {
    var context = CreateContext(handlerKind);

    _ = context.ReminderStore
      .Setup(x => x.GetByToDoIdAsync(context.ToDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<Reminder>>([]));

    SetupPrimaryAction(context, Result.Ok());

    var result = await ExecuteAsync(context);

    Assert.True(result.IsSuccess);
    context.ReminderStore.Verify(x => x.UnlinkFromToDoAsync(It.IsAny<ReminderId>(), It.IsAny<ToDoId>(), It.IsAny<CancellationToken>()), Times.Never);
    context.Scheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Theory]
  [MemberData(nameof(HandlerKinds))]
  public async Task HandleWhenReminderHasNoRemainingLinksDeletesArtifactsAsync(string handlerKind)
  {
    var context = CreateContext(handlerKind);
    var reminder = ApplicationTestData.CreateReminder(quartzJobId: context.QuartzJobId);

    _ = context.ReminderStore
      .Setup(x => x.GetByToDoIdAsync(context.ToDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<Reminder>>([reminder]));

    SetupPrimaryAction(context, Result.Ok());

    _ = context.ReminderStore
      .Setup(x => x.UnlinkFromToDoAsync(reminder.Id, context.ToDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = context.ReminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminder.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDoId>>([]));

    _ = context.Scheduler
      .Setup(x => x.DeleteJobAsync(context.QuartzJobId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = context.ReminderStore
      .Setup(x => x.DeleteAsync(reminder.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    var result = await ExecuteAsync(context);

    Assert.True(result.IsSuccess);
    context.ReminderStore.Verify(x => x.UnlinkFromToDoAsync(reminder.Id, context.ToDoId, It.IsAny<CancellationToken>()), Times.Once);
    context.Scheduler.Verify(x => x.DeleteJobAsync(context.QuartzJobId, It.IsAny<CancellationToken>()), Times.Once);
    context.ReminderStore.Verify(x => x.DeleteAsync(reminder.Id, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Theory]
  [MemberData(nameof(HandlerKinds))]
  public async Task HandleWhenReminderStillHasLinksSkipsDeletionAsync(string handlerKind)
  {
    var context = CreateContext(handlerKind);
    var otherToDoId = new ToDoId(Guid.NewGuid());
    var reminder = ApplicationTestData.CreateReminder(quartzJobId: context.QuartzJobId);

    _ = context.ReminderStore
      .Setup(x => x.GetByToDoIdAsync(context.ToDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<Reminder>>([reminder]));

    SetupPrimaryAction(context, Result.Ok());

    _ = context.ReminderStore
      .Setup(x => x.UnlinkFromToDoAsync(reminder.Id, context.ToDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    _ = context.ReminderStore
      .Setup(x => x.GetLinkedToDoIdsAsync(reminder.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok<IEnumerable<ToDoId>>([otherToDoId]));

    _ = context.Scheduler
      .Setup(x => x.GetOrCreateJobAsync(reminder.ReminderTime.Value, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(context.QuartzJobId));

    var result = await ExecuteAsync(context);

    Assert.True(result.IsSuccess);
    context.ReminderStore.Verify(x => x.UnlinkFromToDoAsync(reminder.Id, context.ToDoId, It.IsAny<CancellationToken>()), Times.Once);
    context.Scheduler.Verify(x => x.DeleteJobAsync(It.IsAny<QuartzJobId>(), It.IsAny<CancellationToken>()), Times.Never);
    context.ReminderStore.Verify(x => x.DeleteAsync(It.IsAny<ReminderId>(), It.IsAny<CancellationToken>()), Times.Never);
    context.Scheduler.Verify(x => x.GetOrCreateJobAsync(reminder.ReminderTime.Value, It.IsAny<CancellationToken>()), Times.Once);
  }

  private static ReminderCleanupContext CreateContext(string handlerKind)
  {
    return new ReminderCleanupContext(handlerKind, new ToDoId(Guid.NewGuid()), new QuartzJobId(Guid.NewGuid()));
  }

  private static void SetupPrimaryAction(ReminderCleanupContext context, Result result)
  {
    if (context.HandlerKind == "delete")
    {
      _ = context.ToDoStore
        .Setup(x => x.DeleteAsync(context.ToDoId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(result);
      return;
    }

    _ = context.ToDoStore
      .Setup(x => x.CompleteAsync(context.ToDoId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(result);
  }

  private static Task<Result> ExecuteAsync(ReminderCleanupContext context)
  {
    return context.HandlerKind == "delete"
      ? context.DeleteHandler.Handle(new DeleteToDoCommand(context.ToDoId), CancellationToken.None)
      : context.CompleteHandler.Handle(new CompleteToDoCommand(context.ToDoId), CancellationToken.None);
  }

  private sealed record ReminderCleanupContext(string HandlerKind, ToDoId ToDoId, QuartzJobId QuartzJobId)
  {
    public Mock<IToDoStore> ToDoStore { get; } = new();
    public Mock<IReminderStore> ReminderStore { get; } = new();
    public Mock<IToDoReminderScheduler> Scheduler { get; } = new();
    public Mock<IMediator> Mediator { get; } = new();
    public Mock<ILogger<DeleteToDoCommandHandler>> DeleteLogger { get; } = new();
    public Mock<ILogger<CompleteToDoCommandHandler>> CompleteLogger { get; } = new();

    public DeleteToDoCommandHandler DeleteHandler => new(ToDoStore.Object, ReminderStore.Object, Scheduler.Object, Mediator.Object, DeleteLogger.Object);
    public CompleteToDoCommandHandler CompleteHandler => new(ToDoStore.Object, ReminderStore.Object, Scheduler.Object, Mediator.Object, CompleteLogger.Object);
  }
}
