using Apollo.API.Jobs;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using Moq;

using Quartz;

namespace Apollo.API.Tests.Jobs;

public class QuartzToDoReminderSchedulerTests
{
  private const string ToDoReminderGroup = "todo-reminders";

  [Fact]
  public async Task DeleteJobAsyncDeletesJobKeyInTodoRemindersGroup()
  {
    var quartzJobId = new QuartzJobId(Guid.NewGuid());

    var scheduler = new Mock<IScheduler>();
    _ = scheduler
      .Setup(x => x.DeleteJob(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);

    var schedulerFactory = new Mock<ISchedulerFactory>();
    _ = schedulerFactory
      .Setup(x => x.GetScheduler(It.IsAny<CancellationToken>()))
      .ReturnsAsync(scheduler.Object);

    var sut = new QuartzToDoReminderScheduler(schedulerFactory.Object);

    var result = await sut.DeleteJobAsync(quartzJobId, CancellationToken.None);

    Assert.True(result.IsSuccess);
    scheduler.Verify(
      x => x.DeleteJob(
        It.Is<JobKey>(k => k.Group == ToDoReminderGroup && k.Name == quartzJobId.Value.ToString("N")),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task DeleteJobAsyncWhenSchedulerFactoryThrowsReturnsFail()
  {
    var schedulerFactory = new Mock<ISchedulerFactory>();
    _ = schedulerFactory
      .Setup(x => x.GetScheduler(It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("boom"));

    var sut = new QuartzToDoReminderScheduler(schedulerFactory.Object);

    var result = await sut.DeleteJobAsync(new QuartzJobId(Guid.NewGuid()), CancellationToken.None);

    Assert.True(result.IsFailed);
    Assert.Equal("boom", result.Errors[0].Message);
  }

  [Fact]
  public async Task GetOrCreateJobAsyncWhenJobDoesNotExistSchedulesJobWithTriggerAtReminderTime()
  {
    var reminderDate = new DateTime(2025, 01, 01, 12, 00, 00, DateTimeKind.Utc);

    JobKey? capturedJobKey = null;
    ITrigger? capturedTrigger = null;
    Type? capturedJobType = null;

    var scheduler = new Mock<IScheduler>();
    _ = scheduler
      .Setup(x => x.CheckExists(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(false);

    _ = scheduler
      .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
      .Callback<IJobDetail, ITrigger, CancellationToken>((job, trigger, _) =>
      {
        capturedJobKey = job.Key;
        capturedJobType = job.JobType;
        capturedTrigger = trigger;
      })
      .ReturnsAsync(DateTimeOffset.UtcNow);

    var schedulerFactory = new Mock<ISchedulerFactory>();
    _ = schedulerFactory
      .Setup(x => x.GetScheduler(It.IsAny<CancellationToken>()))
      .ReturnsAsync(scheduler.Object);

    var sut = new QuartzToDoReminderScheduler(schedulerFactory.Object);

    var result = await sut.GetOrCreateJobAsync(reminderDate, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.NotNull(capturedJobKey);
    Assert.NotNull(capturedTrigger);
    Assert.Equal(ToDoReminderGroup, capturedJobKey.Group);
    Assert.Equal(typeof(ToDoReminderJob), capturedJobType);

    Assert.Equal(ToDoReminderGroup, capturedTrigger.Key.Group);
    Assert.Equal($"{capturedJobKey.Name}-trigger", capturedTrigger.Key.Name);

    var reminderUtc = new UtcDateTime(reminderDate);
    Assert.Equal(new DateTimeOffset(reminderUtc, TimeSpan.Zero), capturedTrigger.StartTimeUtc);
  }

  [Fact]
  public async Task GetOrCreateJobAsyncWhenJobExistsDoesNotScheduleJob()
  {
    var reminderDate = new DateTime(2025, 01, 01, 12, 00, 00, DateTimeKind.Utc);

    JobKey? capturedJobKey = null;

    var scheduler = new Mock<IScheduler>();
    _ = scheduler
      .Setup(x => x.CheckExists(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
      .Callback<JobKey, CancellationToken>((jobKey, _) => capturedJobKey = jobKey)
      .ReturnsAsync(true);

    var schedulerFactory = new Mock<ISchedulerFactory>();
    _ = schedulerFactory
      .Setup(x => x.GetScheduler(It.IsAny<CancellationToken>()))
      .ReturnsAsync(scheduler.Object);

    var sut = new QuartzToDoReminderScheduler(schedulerFactory.Object);

    var result = await sut.GetOrCreateJobAsync(reminderDate, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.NotNull(capturedJobKey);
    Assert.Equal(ToDoReminderGroup, capturedJobKey.Group);

    scheduler.Verify(
      x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task GetOrCreateJobAsyncWhenSchedulerFactoryThrowsReturnsFail()
  {
    var schedulerFactory = new Mock<ISchedulerFactory>();
    _ = schedulerFactory
      .Setup(x => x.GetScheduler(It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("boom"));

    var sut = new QuartzToDoReminderScheduler(schedulerFactory.Object);

    var result = await sut.GetOrCreateJobAsync(DateTime.UtcNow, CancellationToken.None);

    Assert.True(result.IsFailed);
    Assert.Equal("boom", result.Errors[0].Message);
  }
}
