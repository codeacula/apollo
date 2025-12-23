using Apollo.Core;
using Apollo.Core.Logging;
using Apollo.Core.Notifications;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.ToDos.ValueObjects;

using Quartz;

namespace Apollo.Service.Jobs;

[DisallowConcurrentExecution]
public class ToDoReminderJob(
  IToDoStore toDoStore,
  IPersonStore personStore,
  IPersonNotificationClient notificationClient,
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
      var dueToDosResult = await toDoStore.GetToDosByQuartzJobIdAsync(jobId, context.CancellationToken);

      if (dueToDosResult.IsFailed)
      {
        ToDoLogs.LogFailedToRetrieveToDos(logger, dueToDosResult.GetErrorMessages());
        return;
      }

      var dueToDos = dueToDosResult.Value.ToList();
      ToDoLogs.LogFoundDueToDos(logger, dueToDos.Count);

      foreach (var group in dueToDos.GroupBy(t => t.PersonId))
      {
        var firstToDo = group.First();

        try
        {
          var personResult = await personStore.GetAsync(firstToDo.PersonId, context.CancellationToken);
          if (personResult.IsFailed)
          {
            ToDoLogs.LogFailedToGetPerson(logger, firstToDo.PersonId.Value, firstToDo.Id.Value);
            continue;
          }

          var person = personResult.Value;

          if (person.Username.Platform == Platform.Discord)
          {
            var reminderMessage = string.Join("\n", group.Select(t => $"• {t.Description.Value}"));
            var notification = new Notification
            {
              Content = $"**Reminder: You have {group.Count()} ToDo(s) due:**\n{reminderMessage}"
            };

            ToDoLogs.LogSendingGroupedReminder(logger, group.Count(), person.Username.Value);

            var sendResult = await notificationClient.SendNotificationAsync(person, notification, context.CancellationToken);

            if (sendResult.IsFailed)
            {
              ToDoLogs.LogErrorProcessingReminder(logger, new InvalidOperationException(sendResult.GetErrorMessages()), firstToDo.Id.Value);
            }
            else
            {
              ToDoLogs.LogReminder(logger, person.Username.Value, reminderMessage);
            }
          }
        }
        catch (Exception ex)
        {
          ToDoLogs.LogErrorProcessingReminder(logger, ex, firstToDo.Id.Value);
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
