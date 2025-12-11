using Apollo.API.Jobs;
using Apollo.API.Services;
using Apollo.Domain.ToDos.ValueObjects;

using Moq;

using Quartz;

namespace Apollo.API.Tests.Services;

public class ToDoReminderSchedulerTests
{
  private readonly Mock<ISchedulerFactory> _mockSchedulerFactory;
  private readonly Mock<IScheduler> _mockScheduler;
  private readonly ToDoReminderScheduler _scheduler;

  public ToDoReminderSchedulerTests()
  {
    _mockSchedulerFactory = new Mock<ISchedulerFactory>();
    _mockScheduler = new Mock<IScheduler>();
    _mockSchedulerFactory
      .Setup(x => x.GetScheduler(It.IsAny<CancellationToken>()))
      .ReturnsAsync(_mockScheduler.Object);
    _scheduler = new ToDoReminderScheduler(_mockSchedulerFactory.Object);
  }

  [Fact]
  public async Task ScheduleReminderAsyncCreatesJobSuccessfully()
  {
    // Arrange
    var toDoId = new ToDoId(Guid.NewGuid());
    var reminderTime = DateTime.UtcNow.AddHours(1);

    _ = _mockScheduler
      .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(DateTimeOffset.UtcNow);

    // Act
    var result = await _scheduler.ScheduleReminderAsync(toDoId, reminderTime, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotEqual(Guid.Empty, result.Value.Value);
    _mockScheduler.Verify(
      x => x.ScheduleJob(
        It.Is<IJobDetail>(job => job.JobType == typeof(SingleToDoReminderJob)),
        It.IsAny<ITrigger>(),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task CancelReminderAsyncDeletesJobSuccessfully()
  {
    // Arrange
    var quartzJobId = new QuartzJobId(Guid.NewGuid());

    _ = _mockScheduler
      .Setup(x => x.DeleteJob(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);

    // Act
    var result = await _scheduler.CancelReminderAsync(quartzJobId, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    _mockScheduler.Verify(
      x => x.DeleteJob(It.Is<JobKey>(key => key.Name == quartzJobId.Value.ToString()), It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task CancelReminderAsyncWhenJobNotFoundReturnsFailure()
  {
    // Arrange
    var quartzJobId = new QuartzJobId(Guid.NewGuid());

    _ = _mockScheduler
      .Setup(x => x.DeleteJob(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(false);

    // Act
    var result = await _scheduler.CancelReminderAsync(quartzJobId, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Failed to cancel reminder job", result.Errors[0].Message);
  }

  [Fact]
  public async Task ScheduleReminderAsyncWhenExceptionOccursReturnsFailure()
  {
    // Arrange
    var toDoId = new ToDoId(Guid.NewGuid());
    var reminderTime = DateTime.UtcNow.AddHours(1);

    _ = _mockScheduler
      .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(new Exception("Scheduler error"));

    // Act
    var result = await _scheduler.ScheduleReminderAsync(toDoId, reminderTime, CancellationToken.None);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Scheduler error", result.Errors[0].Message);
  }
}
