using System.Globalization;
using System.Text;
using System.Text.Json;

using Apollo.AI;
using Apollo.Application.ToDos.Models;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record GetDailyPlanQuery(PersonId PersonId) : IRequest<Result<DailyPlan>>;

public sealed class GetDailyPlanQueryHandler(
  IToDoStore toDoStore,
  IPersonStore personStore,
  IApolloAIAgent aiAgent,
  TimeProvider timeProvider,
  PersonConfig personConfig) : IRequestHandler<GetDailyPlanQuery, Result<DailyPlan>>
{
  public async Task<Result<DailyPlan>> Handle(GetDailyPlanQuery request, CancellationToken cancellationToken)
  {
    try
    {
      var todosResult = await GetActiveToDosAsync(request.PersonId, cancellationToken);
      if (todosResult.IsFailed)
      {
        return todosResult.ToResult<DailyPlan>();
      }

      if (todosResult.Value.Count == 0)
      {
        return Result.Ok(new DailyPlan([], "You have no active todos! 🎉", 0));
      }

      var dailyTaskCount = await GetDailyTaskCountAsync(request.PersonId, cancellationToken);
      if (todosResult.Value.Count <= dailyTaskCount)
      {
        return Result.Ok(CreateAllTodosPlan(todosResult.Value));
      }

      var (timeZoneId, localTime) = await GetUserTimeContextAsync(request.PersonId, cancellationToken);
      return await GenerateAIPlanAsync(todosResult.Value, timeZoneId, localTime, dailyTaskCount, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail<DailyPlan>($"An error occurred while generating daily plan: {ex.Message}");
    }
  }

  private async Task<Result<List<ToDo>>> GetActiveToDosAsync(PersonId personId, CancellationToken cancellationToken)
  {
    var todosResult = await toDoStore.GetByPersonIdAsync(personId, includeCompleted: false, cancellationToken);
    return todosResult.IsFailed ? Result.Fail<List<ToDo>>(todosResult.Errors) : Result.Ok(todosResult.Value.ToList());
  }

  private async Task<int> GetDailyTaskCountAsync(PersonId personId, CancellationToken cancellationToken)
  {
    var personResult = await personStore.GetAsync(personId, cancellationToken);
    return personResult.IsSuccess && personResult.Value.DailyTaskCount.HasValue
      ? personResult.Value.DailyTaskCount.Value.Value
      : personConfig.DefaultDailyTaskCount;
  }

  private static DailyPlan CreateAllTodosPlan(List<ToDo> todos)
  {
    var allItems = todos.ConvertAll(t => new DailyPlanItem(
      t.Id,
      t.Description.Value,
      t.Priority,
      t.Energy,
      t.Interest,
      t.DueDate?.Value
    ));

    return new DailyPlan(
      allItems,
      $"You have {todos.Count} active todo{(todos.Count == 1 ? "" : "s")} - here they all are!",
      todos.Count
    );
  }

  private async Task<(string TimeZoneId, string LocalTime)> GetUserTimeContextAsync(
    PersonId personId,
    CancellationToken cancellationToken)
  {
    var personResult = await personStore.GetAsync(personId, cancellationToken);
    var timeZoneId = personResult.IsSuccess && personResult.Value.TimeZoneId.HasValue
      ? personResult.Value.TimeZoneId.Value.Value
      : personConfig.DefaultTimeZoneId;

    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    var utcNow = timeProvider.GetUtcNow().UtcDateTime;
    var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZoneInfo);

    return (timeZoneId, localTime.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
  }

  private async Task<Result<DailyPlan>> GenerateAIPlanAsync(List<ToDo> todos, string timeZoneId, string localTime, int dailyTaskCount, CancellationToken cancellationToken)
  {
    // Format todos for AI
    var todosFormatted = FormatToDosForAI(todos);

    // Call AI agent
    var aiResult = await (await aiAgent
      .CreateDailyPlanRequestAsync(
        timeZoneId,
        localTime,
        todosFormatted,
        dailyTaskCount
      ))
      .ExecuteAsync(cancellationToken);

    if (!aiResult.Success)
    {
      return Result.Fail<DailyPlan>($"Failed to generate daily plan: {aiResult.ErrorMessage}");
    }

    // Parse JSON response
    var parseResult = ParseAIResponse(aiResult.Content, todos);
    return parseResult.IsFailed
      ? Result.Fail<DailyPlan>(parseResult.Errors)
      : Result.Ok(new DailyPlan(
        parseResult.Value.Tasks,
        parseResult.Value.Rationale,
        todos.Count
      ));
  }

  private static string FormatToDosForAI(List<ToDo> todos)
  {
    var sb = new StringBuilder();
    foreach (var todo in todos)
    {
      var priority = LevelToEmoji(todo.Priority.Value);
      var energy = LevelToEmoji(todo.Energy.Value);
      var interest = LevelToEmoji(todo.Interest.Value);
      var dueDate = todo.DueDate.HasValue
        ? $" Due: {todo.DueDate.Value.Value:yyyy-MM-dd}"
        : "";

      _ = sb.AppendLine(CultureInfo.InvariantCulture, $"- [{todo.Id.Value}] {todo.Description.Value} (P:{priority} E:{energy} I:{interest}){dueDate}");
    }

    return sb.ToString();
  }

  private static string LevelToEmoji(Level level) => level switch
  {
    Level.Blue => "🔵",
    Level.Green => "🟢",
    Level.Yellow => "🟡",
    Level.Red => "🔴",
    _ => "⚪"
  };

  private static Result<(List<DailyPlanItem> Tasks, string Rationale)> ParseAIResponse(string jsonContent, List<ToDo> allTodos)
  {
    try
    {
      // Parse JSON
      using var doc = JsonDocument.Parse(jsonContent);
      var root = doc.RootElement;

      if (!root.TryGetProperty("selected_task_ids", out var idsElement))
      {
        return Result.Fail<(List<DailyPlanItem>, string)>("AI response missing 'selected_task_ids' field");
      }

      if (!root.TryGetProperty("rationale", out var rationaleElement))
      {
        return Result.Fail<(List<DailyPlanItem>, string)>("AI response missing 'rationale' field");
      }

      var rationale = rationaleElement.GetString() ?? "";

      // Create a dictionary for quick lookup
      var todoDict = allTodos.ToDictionary(t => t.Id.Value.ToString(), t => t);

      // Map task IDs to full DailyPlanItem objects, preserving AI's order
      var tasks = idsElement
        .EnumerateArray()
        .Select(idElement => idElement.GetString())
        .Where(idStr => idStr != null && todoDict.TryGetValue(idStr, out _))
        .Select(idStr =>
        {
          var todo = todoDict[idStr!];
          return new DailyPlanItem(
            todo.Id,
            todo.Description.Value,
            todo.Priority,
            todo.Energy,
            todo.Interest,
            todo.DueDate?.Value
          );
        })
        .ToList();

      return tasks.Count == 0 ? (Result<(List<DailyPlanItem> Tasks, string Rationale)>)Result.Fail<(List<DailyPlanItem>, string)>("AI returned no valid task IDs") : (Result<(List<DailyPlanItem> Tasks, string Rationale)>)Result.Ok((tasks, rationale));
    }
    catch (JsonException ex)
    {
      return Result.Fail<(List<DailyPlanItem>, string)>($"Invalid JSON format: {ex.Message}");
    }
  }
}
