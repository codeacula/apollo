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

      // Get reminders by QuartzJobId
      var remindersResult = await reminderStore.GetByQuartzJobIdAsync(jobId, context.CancellationToken);

      if (remindersResult.IsFailed)
      {
        ToDoLogs.LogFailedToRetrieveToDos(logger, remindersResult.GetErrorMessages());
        return;
      }

      var reminders = remindersResult.Value.ToList();
      ToDoLogs.LogFoundDueToDos(logger, reminders.Count);

      // Collect all ToDos linked to these reminders, grouped by PersonId
      var toDosByPerson = new Dictionary<Guid, List<(Domain.ToDos.Models.ToDo ToDo, Domain.ToDos.Models.Reminder Reminder)>>();

      foreach (var reminder in reminders)
      {
        var linkedToDoIdsResult = await reminderStore.GetLinkedToDoIdsAsync(reminder.Id, context.CancellationToken);
        if (linkedToDoIdsResult.IsFailed)
        {
          continue;
        }

        foreach (var toDoId in linkedToDoIdsResult.Value)
        {
          var toDoResult = await toDoStore.GetAsync(toDoId, context.CancellationToken);
          if (toDoResult.IsFailed)
          {
            continue;
          }

          var toDo = toDoResult.Value;
          var personId = toDo.PersonId.Value;

          if (!toDosByPerson.TryGetValue(personId, out var todoList))
          {
            todoList = [];
            toDosByPerson[personId] = todoList;
          }

          todoList.Add((toDo, reminder));
        }
      }

      // Send notifications grouped by person
      foreach (var (personId, todoReminderPairs) in toDosByPerson)
      {
        var firstToDo = todoReminderPairs[0].ToDo;

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
            var toDoDescriptions = todoReminderPairs.Select(p => p.ToDo.Description.Value);

            // Use AI to generate a personalized reminder message
            var messageResult = await reminderMessageGenerator.GenerateReminderMessageAsync(
              person.Username.Value,
              toDoDescriptions,
              context.CancellationToken);

            string reminderContent;
            if (messageResult.IsSuccess)
            {
              reminderContent = messageResult.Value;
            }
            else
            {
              // Fallback to static message if AI generation fails
              ToDoLogs.LogErrorProcessingReminder(logger, new InvalidOperationException($"AI message generation failed: {messageResult.GetErrorMessages()}"), firstToDo.Id.Value);
              var reminderMessage = string.Join("\n", toDoDescriptions.Select(d => $"• {d}"));
              reminderContent = $"**Reminder: You have {todoReminderPairs.Count} ToDo(s) due:**\n{reminderMessage}";
            }

            var notification = new Notification
            {
              Content = reminderContent
            };

            ToDoLogs.LogSendingGroupedReminder(logger, todoReminderPairs.Count, person.Username.Value);

            var sendResult = await notificationClient.SendNotificationAsync(person, notification, context.CancellationToken);

            if (sendResult.IsFailed)
            {
              ToDoLogs.LogErrorProcessingReminder(logger, new InvalidOperationException(sendResult.GetErrorMessages()), firstToDo.Id.Value);
            }
            else
            {
              ToDoLogs.LogReminder(logger, person.Username.Value, reminderContent);

              // Mark reminders as sent
              foreach (var (_, reminder) in todoReminderPairs)
              {
                _ = await reminderStore.MarkAsSentAsync(reminder.Id, context.CancellationToken);
              }
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
