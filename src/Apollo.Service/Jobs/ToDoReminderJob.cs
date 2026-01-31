using Apollo.Core;
using Apollo.Core.Logging;
using Apollo.Core.Notifications;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.ValueObjects;

using Quartz;

namespace Apollo.Service.Jobs;

[DisallowConcurrentExecution]
public class ToDoReminderJob(
  IReminderStore reminderStore,
  IPersonStore personStore,
  IPersonNotificationClient notificationClient,
  IReminderMessageGenerator reminderMessageGenerator,
  ILogger<ToDoReminderJob> logger,
  TimeProvider timeProvider) : IJob
{
  public async Task Execute(IJobExecutionContext context)
  {
    try
    {
      ToDoLogs.LogJobStarted(logger, timeProvider.GetUtcNow());

      if (!Guid.TryParse(context.JobDetail.Key.Name, out var jobGuid))
      {
        ToDoLogs.LogFailedToRetrieveToDos(logger, $"Invalid Quartz job id: {context.JobDetail.Key.Name}");
        return;
      }

      var jobId = new QuartzJobId(jobGuid);

      var remindersResult = await reminderStore.GetByQuartzJobIdAsync(jobId, context.CancellationToken);

      if (remindersResult.IsFailed)
      {
        ToDoLogs.LogFailedToRetrieveToDos(logger, remindersResult.GetErrorMessages());
        return;
      }

      var reminders = remindersResult.Value.ToList();
      ToDoLogs.LogFoundDueToDos(logger, reminders.Count);

      foreach (var personGroup in reminders.GroupBy(r => r.PersonId.Value))
      {
        var personId = personGroup.Key;
        var personReminders = personGroup.ToList();

        try
        {
          var personResult = await personStore.GetAsync(new(personId), context.CancellationToken);
          if (personResult.IsFailed)
          {
            ToDoLogs.LogFailedToGetPerson(logger, personId.ToString(), personReminders[0].Id.Value);
            continue;
          }

          var person = personResult.Value;

          var reminderDetails = personReminders.Select(r => r.Details.Value);

          var messageResult = await reminderMessageGenerator.GenerateReminderMessageAsync(
            person,
            reminderDetails,
            context.CancellationToken);

          string reminderContent;
          if (messageResult.IsSuccess)
          {
            reminderContent = messageResult.Value;
          }
          else
          {
            ToDoLogs.LogErrorProcessingReminder(logger, new InvalidOperationException($"AI message generation failed: {messageResult.GetErrorMessages()}"), personReminders[0].Id.Value);
            var reminderMessage = string.Join("\n", reminderDetails.Select(d => $"• {d}"));
            reminderContent = $"**Reminder: You have {personReminders.Count} reminder(s) due:**\n{reminderMessage}";
          }

          var notification = new Notification
          {
            Content = reminderContent
          };

          ToDoLogs.LogSendingGroupedReminder(logger, personReminders.Count, person.Username.Value);

          var sendResult = await notificationClient.SendNotificationAsync(person, notification, context.CancellationToken);

          if (sendResult.IsFailed)
          {
            ToDoLogs.LogErrorProcessingReminder(logger, new InvalidOperationException(sendResult.GetErrorMessages()), personReminders[0].Id.Value);
            continue;
          }

          ToDoLogs.LogReminder(logger, person.Username.Value, reminderContent);

          foreach (var reminder in personReminders)
          {
            var markAsSentResult = await reminderStore.MarkAsSentAsync(reminder.Id, context.CancellationToken);
            if (markAsSentResult.IsFailed)
            {
              ToDoLogs.LogFailedToMarkReminderAsSent(logger, reminder.Id.Value, markAsSentResult.GetErrorMessages());
            }
          }
        }
        catch (Exception ex)
        {
          ToDoLogs.LogErrorProcessingReminder(logger, ex, personReminders[0].Id.Value);
        }
      }

      _ = await context.Scheduler.DeleteJob(context.JobDetail.Key, context.CancellationToken);

      ToDoLogs.LogJobCompleted(logger, timeProvider.GetUtcNow());
    }
    catch (Exception ex)
    {
      ToDoLogs.LogJobFailed(logger, ex);
    }
  }
}
