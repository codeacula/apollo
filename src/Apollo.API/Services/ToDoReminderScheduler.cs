using Apollo.API.Jobs;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Quartz;

namespace Apollo.API.Services;

public class ToDoReminderScheduler(ISchedulerFactory schedulerFactory, IToDoStore toDoStore) : IToDoReminderScheduler
{
  public async Task<Result> CancelReminderAsync(QuartzJobId quartzJobId, CancellationToken cancellationToken = default)
  {
    try
    {
      var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
      var jobKey = new JobKey(quartzJobId.Value.ToString());

      // Check if any other ToDos are using this same reminder time
      var dueTasksResult = await toDoStore.GetDueTasksAsync(DateTime.UtcNow.AddYears(100), cancellationToken);
      if (dueTasksResult.IsSuccess)
      {
        var todosWithSameJobId = dueTasksResult.Value.Count(t => t.Reminders.Any(r =>
          r.QuartzJobId.HasValue &&
          r.QuartzJobId.Value.Value == quartzJobId.Value
        ));

        // Don't delete the job if other ToDos are still using it
        if (todosWithSameJobId > 1)
        {
          return Result.Ok();
        }
      }

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

      // Round to the nearest minute to allow job sharing
      var roundedTime = new DateTime(reminderTime.Year, reminderTime.Month, reminderTime.Day,
                                      reminderTime.Hour, reminderTime.Minute, 0, DateTimeKind.Utc);

      // Create a deterministic GUID based on the reminder time
      var jobGuid = CreateDeterministicGuid(roundedTime);
      var jobKey = new JobKey(jobGuid.ToString());

      // Check if a job already exists for this time
      var existingJob = await scheduler.GetJobDetail(jobKey, cancellationToken);
      if (existingJob == null)
      {
        // Create a new job for this time slot that will process all ToDos due at this time
        var job = JobBuilder.Create<ToDoReminderJob>()
          .WithIdentity(jobKey)
          .StoreDurably()
          .Build();

        var trigger = TriggerBuilder.Create()
          .WithIdentity($"trigger-{jobGuid}")
          .ForJob(jobKey)
          .StartAt(roundedTime)
          .Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken);
      }

      return Result.Ok(new QuartzJobId(jobGuid));
    }
    catch (Exception ex)
    {
      return Result.Fail<QuartzJobId>(ex.Message);
    }
  }

  private static Guid CreateDeterministicGuid(DateTime reminderTime)
  {
    // Create a deterministic GUID based on the reminder time
    // Format: yyyyMMddHHmm as a string, then hash to create GUID
    var timeString = $"reminder-{reminderTime:yyyyMMddHHmm}";
#pragma warning disable CA5351 // MD5 is used for non-cryptographic purposes (deterministic GUID generation)
    var guidBytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(timeString));
#pragma warning restore CA5351
    return new Guid(guidBytes);
  }
}
