using Apollo.Core.Logging;
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
        ToDoLogs.LogFailedToRetrieveToDos(logger, string.Join(", ", dueToDosResult.Errors.Select(e => e.Message)));
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
            ToDoLogs.LogSendingGroupedReminder(logger, group.Count(), person.Username.Value);
            ToDoLogs.LogReminder(logger, person.Username.Value, string.Join("\n", group.Select(t => t.Description.Value)));
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
