using Apollo.Core.Logging;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.ToDos.ValueObjects;

using Quartz;

namespace Apollo.API.Jobs;

[DisallowConcurrentExecution]
public class SingleToDoReminderJob(
  IToDoStore toDoStore,
  IPersonStore personStore,
  ILogger<SingleToDoReminderJob> logger) : IJob
{
  public async Task Execute(IJobExecutionContext context)
  {
    try
    {
      var toDoIdString = context.JobDetail.JobDataMap.GetString("ToDoId");
      if (string.IsNullOrEmpty(toDoIdString) || !Guid.TryParse(toDoIdString, out var toDoIdGuid))
      {
        logger.LogError("SingleToDoReminderJob executed without valid ToDoId");
        return;
      }

      var toDoId = new ToDoId(toDoIdGuid);
      var toDoResult = await toDoStore.GetAsync(toDoId, context.CancellationToken);

      if (toDoResult.IsFailed)
      {
        ToDoLogs.LogFailedToRetrieveToDos(logger, string.Join(", ", toDoResult.Errors.Select(e => e.Message)));
        return;
      }

      var todo = toDoResult.Value;
      var personResult = await personStore.GetAsync(todo.PersonId, context.CancellationToken);

      if (personResult.IsFailed)
      {
        ToDoLogs.LogFailedToGetPerson(logger, todo.PersonId.Value, todo.Id.Value);
        return;
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
      logger.LogError(ex, "Error executing SingleToDoReminderJob");
    }
  }
}
