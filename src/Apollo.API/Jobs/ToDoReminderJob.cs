using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;

using Quartz;

namespace Apollo.API.Jobs;

[DisallowConcurrentExecution]
public partial class ToDoReminderJob(
  IToDoStore toDoStore,
  IPersonStore personStore,
  ILogger<ToDoReminderJob> logger,
  TimeProvider timeProvider) : IJob
{
  public async Task Execute(IJobExecutionContext context)
  {
    try
    {
      LogJobStarted(timeProvider.GetUtcNow());

      var currentTime = timeProvider.GetUtcNow().DateTime;
      var dueTasksResult = await toDoStore.GetDueTasksAsync(currentTime, context.CancellationToken);

      if (dueTasksResult.IsFailed)
      {
        LogFailedToRetrieveToDos(string.Join(", ", dueTasksResult.Errors.Select(e => e.Message)));
        return;
      }

      var dueToDos = dueTasksResult.Value.ToList();
      LogFoundDueToDos(dueToDos.Count);

      foreach (var todo in dueToDos)
      {
        try
        {
          var personResult = await personStore.GetAsync(todo.PersonId, context.CancellationToken);
          if (personResult.IsFailed)
          {
            LogFailedToGetPerson(todo.PersonId.Value, todo.Id.Value);
            continue;
          }

          var person = personResult.Value;

          if (person.Username.Platform == Platform.Discord)
          {
            LogSendingReminder(todo.Id.Value, person.Username.Value);
            LogReminder(person.Username.Value, todo.Description.Value);
          }
        }
        catch (Exception ex)
        {
          LogErrorProcessingReminder(ex, todo.Id.Value);
        }
      }

      LogJobCompleted(timeProvider.GetUtcNow());
    }
    catch (Exception ex)
    {
      LogJobFailed(ex);
    }
  }

  [LoggerMessage(Level = LogLevel.Information, Message = "ToDoReminderJob started at {Time}")]
  private partial void LogJobStarted(DateTimeOffset time);

  [LoggerMessage(Level = LogLevel.Error, Message = "Failed to retrieve due todos: {Errors}")]
  private partial void LogFailedToRetrieveToDos(string errors);

  [LoggerMessage(Level = LogLevel.Information, Message = "Found {Count} todos with due reminders")]
  private partial void LogFoundDueToDos(int count);

  [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to get person {PersonId} for todo {ToDoId}")]
  private partial void LogFailedToGetPerson(Guid personId, Guid toDoId);

  [LoggerMessage(Level = LogLevel.Information, Message = "Sending reminder for todo {ToDoId} to user {Username}")]
  private partial void LogSendingReminder(Guid toDoId, string username);

  [LoggerMessage(Level = LogLevel.Information, Message = "REMINDER for {Username}: {Description}")]
  private partial void LogReminder(string username, string description);

  [LoggerMessage(Level = LogLevel.Error, Message = "Error processing reminder for todo {ToDoId}")]
  private partial void LogErrorProcessingReminder(Exception ex, Guid toDoId);

  [LoggerMessage(Level = LogLevel.Information, Message = "ToDoReminderJob completed at {Time}")]
  private partial void LogJobCompleted(DateTimeOffset time);

  [LoggerMessage(Level = LogLevel.Error, Message = "ToDoReminderJob failed")]
  private partial void LogJobFailed(Exception ex);
}
