using Apollo.API.Jobs;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Quartz;

namespace Apollo.API.Services;

public class ToDoReminderScheduler(ISchedulerFactory schedulerFactory) : IToDoReminderScheduler
{
  public async Task<Result> CancelReminderAsync(QuartzJobId quartzJobId, CancellationToken cancellationToken = default)
  {
    try
    {
      var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
      var jobKey = new JobKey(quartzJobId.Value.ToString());
      var deleted = await scheduler.DeleteJob(jobKey, cancellationToken);

      return deleted
        ? Result.Ok()
        : Result.Fail($"Failed to cancel reminder job with ID {quartzJobId.Value}");
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<QuartzJobId>> ScheduleReminderAsync(ToDoId toDoId, DateTime reminderTime, CancellationToken cancellationToken = default)
  {
    try
    {
      var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
      var jobId = Guid.NewGuid();
      var jobKey = new JobKey(jobId.ToString());

      var job = JobBuilder.Create<SingleToDoReminderJob>()
        .WithIdentity(jobKey)
        .UsingJobData("ToDoId", toDoId.Value.ToString())
        .Build();

      var trigger = TriggerBuilder.Create()
        .WithIdentity($"trigger-{jobId}")
        .StartAt(reminderTime)
        .Build();

      await scheduler.ScheduleJob(job, trigger, cancellationToken);

      return Result.Ok(new QuartzJobId(jobId));
    }
    catch (Exception ex)
    {
      return Result.Fail<QuartzJobId>(ex.Message);
    }
  }
}
