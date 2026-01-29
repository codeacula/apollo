using System.Globalization;
using System.Text;
using System.Text.Json;

using Apollo.AI;
using Apollo.Application.ToDos.Models;
using Apollo.Application.ToDos.Queries;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.ToDos.Models;

using FluentResults;

namespace Apollo.Application.ToDos.Handlers;

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
      // Step 1: Fetch active todos
      var todosResult = await toDoStore.GetByPersonIdAsync(request.PersonId, includeCompleted: false, cancellationToken);
      if (todosResult.IsFailed)
      {
        return Result.Fail<DailyPlan>(todosResult.Errors);
      }

      var todos = todosResult.Value.ToList();

      // Step 2: Handle edge case - no todos
      if (todos.Count == 0)
      {
        return Result.Ok(new DailyPlan(
          [],
          "You have no active todos! 🎉",
          0
        ));
      }

      // Step 3: Get user's daily task count preference
      var personResult = await personStore.GetAsync(request.PersonId, cancellationToken);
      var dailyTaskCount = personResult.IsSuccess && personResult.Value.DailyTaskCount.HasValue
        ? personResult.Value.DailyTaskCount.Value.Value
        : personConfig.DefaultDailyTaskCount;

      // Step 4: Handle edge case - fewer todos than requested count
      if (todos.Count <= dailyTaskCount)
      {
        var allItems = todos.ConvertAll(t => new DailyPlanItem(
          t.Id,
          t.Description.Value,
          t.Priority,
          t.Energy,
          t.Interest,
          t.DueDate?.Value
        ));

        return Result.Ok(new DailyPlan(
          allItems,
          $"You have {todos.Count} active todo{(todos.Count == 1 ? "" : "s")} - here they all are!",
          todos.Count
        ));
      }

      // Step 5: Get user's timezone
      var timeZoneId = personResult.IsSuccess && personResult.Value.TimeZoneId.HasValue
        ? personResult.Value.TimeZoneId.Value.Value
        : personConfig.DefaultTimeZoneId;

      var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
      var utcNow = timeProvider.GetUtcNow().UtcDateTime;
      var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZoneInfo);

      // Step 6: Format todos for AI
      var todosFormatted = FormatToDosForAI(todos);

      // Step 7: Call AI agent
      var aiResult = await aiAgent
        .CreateDailyPlanRequest(
          timeZoneId,
          localTime.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
          todosFormatted,
          dailyTaskCount
        )
        .ExecuteAsync(cancellationToken);

      if (!aiResult.Success)
      {
        return Result.Fail<DailyPlan>($"Failed to generate daily plan: {aiResult.ErrorMessage}");
      }

      // Step 8: Parse JSON response
      var parseResult = ParseAIResponse(aiResult.Content, todos);
      return parseResult.IsFailed
        ? Result.Fail<DailyPlan>(parseResult.Errors)
        : Result.Ok(new DailyPlan(
        parseResult.Value.Tasks,
        parseResult.Value.Rationale,
        todos.Count
      ));
    }
    catch (Exception ex)
    {
      return Result.Fail<DailyPlan>($"An error occurred while generating daily plan: {ex.Message}");
    }
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

      _ = sb.AppendLine($"- [{todo.Id.Value}] {todo.Description.Value} (P:{priority} E:{energy} I:{interest}){dueDate}");
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
      var tasks = new List<DailyPlanItem>();
      foreach (var idElement in idsElement.EnumerateArray())
      {
        var idStr = idElement.GetString();
        if (idStr != null && todoDict.TryGetValue(idStr, out var todo))
        {
          tasks.Add(new DailyPlanItem(
            todo.Id,
            todo.Description.Value,
            todo.Priority,
            todo.Energy,
            todo.Interest,
            todo.DueDate?.Value
          ));
        }
      }

      return tasks.Count == 0 ? (Result<(List<DailyPlanItem> Tasks, string Rationale)>)Result.Fail<(List<DailyPlanItem>, string)>("AI returned no valid task IDs") : (Result<(List<DailyPlanItem> Tasks, string Rationale)>)Result.Ok((tasks, rationale));
    }
    catch (JsonException ex)
    {
      return Result.Fail<(List<DailyPlanItem>, string)>($"Invalid JSON format: {ex.Message}");
    }
  }
}
