using System.Security.Cryptography;
using System.Text;

using Apollo.Core.ToDos;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Quartz;

namespace Apollo.API.Jobs;

public sealed class QuartzToDoReminderScheduler(ISchedulerFactory schedulerFactory) : IToDoReminderScheduler
{
  private const string ToDoReminderGroup = "todo-reminders";
  private const string ToDoReminderIdNamespace = "todo-reminder";

  public async Task<Result> DeleteJobAsync(QuartzJobId quartzJobId, CancellationToken cancellationToken = default)
  {
    try
    {
      var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
      _ = await scheduler.DeleteJob(CreateJobKey(quartzJobId), cancellationToken);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<QuartzJobId>> GetOrCreateJobAsync(DateTime reminderDate, CancellationToken cancellationToken = default)
  {
    try
    {
      var reminderUtc = new UtcDateTime(reminderDate);
      var quartzJobId = CreateQuartzJobId(reminderUtc);

      var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
      var jobKey = CreateJobKey(quartzJobId);

      if (await scheduler.CheckExists(jobKey, cancellationToken))
      {
        return Result.Ok(quartzJobId);
      }

      var job = JobBuilder
        .Create<ToDoReminderJob>()
        .WithIdentity(jobKey)
        .Build();

      var trigger = TriggerBuilder
        .Create()
        .WithIdentity($"{jobKey.Name}-trigger", jobKey.Group)
        .ForJob(jobKey)
        .StartAt(new DateTimeOffset(reminderUtc, TimeSpan.Zero))
        .WithSimpleSchedule(x => x.WithRepeatCount(0).WithMisfireHandlingInstructionFireNow())
        .Build();

      _ = await scheduler.ScheduleJob(job, trigger, cancellationToken);

      return Result.Ok(quartzJobId);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  private static JobKey CreateJobKey(QuartzJobId quartzJobId)
  {
    return new JobKey(quartzJobId.Value.ToString("N"), ToDoReminderGroup);
  }

  private static QuartzJobId CreateQuartzJobId(UtcDateTime reminderUtc)
  {
    var input = $"{ToDoReminderIdNamespace}|{reminderUtc:O}";
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
    return new QuartzJobId(new Guid(hash.AsSpan(0, 16)));
  }
}
