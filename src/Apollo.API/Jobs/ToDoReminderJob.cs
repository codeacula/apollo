using Apollo.Core.Logging;
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
      ToDoLogs.LogJobStarted(logger, timeProvider.GetUtcNow());

      var currentTime = timeProvider.GetUtcNow().DateTime;
      var dueToDosResult = await toDoStore.GetDueToDosAsync(currentTime, context.CancellationToken);

      if (dueToDosResult.IsFailed)
      {
        ToDoLogs.LogFailedToRetrieveToDos(logger, string.Join(", ", dueToDosResult.Errors.Select(e => e.Message)));
        return;
      }

      var dueToDos = dueToDosResult.Value.ToList();
      ToDoLogs.LogFoundDueToDos(logger, dueToDos.Count);

      foreach (var todo in dueToDos)
      {
        try
        {
          var personResult = await personStore.GetAsync(todo.PersonId, context.CancellationToken);
          if (personResult.IsFailed)
          {
            ToDoLogs.LogFailedToGetPerson(logger, todo.PersonId.Value, todo.Id.Value);
            continue;
          }

          var person = personResult.Value;

          if (person.Username.Platform == Platform.Discord)
          {
            ToDoLogs.LogSendingReminder(logger, todo.Id.Value, person.Username.Value);
            ToDoLogs.LogReminder(logger, person.Username.Value, todo.Description.Value);
          }
        }
        catch (Exception ex)
        {
          ToDoLogs.LogErrorProcessingReminder(logger, ex, todo.Id.Value);
        }
      }

      ToDoLogs.LogJobCompleted(logger, timeProvider.GetUtcNow());
    }
    catch (Exception ex)
    {
      ToDoLogs.LogJobFailed(logger, ex);
    }
  }
}
