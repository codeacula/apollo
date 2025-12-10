using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;

using Quartz;

namespace Apollo.API.Jobs;

[DisallowConcurrentExecution]
public class ToDoReminderJob(
  IToDoStore toDoStore,
  IPersonStore personStore,
  ILogger<ToDoReminderJob> logger,
  TimeProvider timeProvider) : IJob
{
  public async Task Execute(IJobExecutionContext context)
  {
    try
    {
      logger.LogInformation("ToDoReminderJob started at {Time}", timeProvider.GetUtcNow());

      var currentTime = timeProvider.GetUtcNow().DateTime;
      var dueTasksResult = await toDoStore.GetDueTasksAsync(currentTime, context.CancellationToken);

      if (dueTasksResult.IsFailed)
      {
        logger.LogError("Failed to retrieve due todos: {Errors}", string.Join(", ", dueTasksResult.Errors.Select(e => e.Message)));
        return;
      }

      var dueToDos = dueTasksResult.Value.ToList();
      logger.LogInformation("Found {Count} todos with due reminders", dueToDos.Count);

      foreach (var todo in dueToDos)
      {
        try
        {
          var personResult = await personStore.GetAsync(todo.PersonId, context.CancellationToken);
          if (personResult.IsFailed)
          {
            logger.LogWarning("Failed to get person {PersonId} for todo {ToDoId}", todo.PersonId.Value, todo.Id.Value);
            continue;
          }

          var person = personResult.Value;

          if (person.Username.Platform == Platform.Discord)
          {
            logger.LogInformation("Sending reminder for todo {ToDoId} to user {Username}",
              todo.Id.Value, person.Username.Value);

            logger.LogInformation("REMINDER for {Username}: {Description}",
              person.Username.Value, todo.Description.Value);
          }
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error processing reminder for todo {ToDoId}", todo.Id.Value);
        }
      }

      logger.LogInformation("ToDoReminderJob completed at {Time}", timeProvider.GetUtcNow());
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "ToDoReminderJob failed");
    }
  }
}
