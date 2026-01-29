using System.ComponentModel;
using System.Text;

using Apollo.Application.ToDos.Commands;
using Apollo.Application.ToDos.Queries;
using Apollo.Core;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Microsoft.SemanticKernel;

namespace Apollo.Application.ToDos;

public sealed class ToDoPlugin(
  IMediator mediator,
  IPersonStore personStore,
  IFuzzyTimeParser fuzzyTimeParser,
  TimeProvider timeProvider,
  PersonConfig personConfig,
  PersonId personId)
{
  public const string PluginName = "ToDos";

  [KernelFunction("create_todo")]
  [Description("Creates a new todo with an optional reminder. Supports fuzzy times like 'in 10 minutes', 'in 2 hours', 'tomorrow', or ISO 8601 format.")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Maintains better code readability")]
  public async Task<string> CreateToDoAsync(
    [Description("The todo description")] string description,
    [Description("Optional reminder time. Supports fuzzy formats like 'in 10 minutes', 'in 2 hours', 'tomorrow', 'next week', or ISO 8601 format (e.g., 2025-12-31T10:00:00).")] string? reminderDate = null,
    [Description("Optional priority level. Accepts: 'blue'/'low' (0), 'green'/'medium' (1, default), 'yellow'/'high' (2), 'red'/'urgent' (3), or color emojis.")] string? priority = null,
    [Description("Optional energy level. Accepts: 'blue'/'low' (0), 'green'/'medium' (1, default), 'yellow'/'high' (2), 'red'/'urgent' (3), or color emojis.")] string? energy = null,
    [Description("Optional interest level. Accepts: 'blue'/'low' (0), 'green'/'medium' (1, default), 'yellow'/'high' (2), 'red'/'urgent' (3), or color emojis.")] string? interest = null,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var reminder = await ParseReminderDateAsync(reminderDate, cancellationToken);
      if (reminder.IsFailed)
      {
        return $"Failed to create todo: {reminder.GetErrorMessages()}";
      }

      var priorityResult = ParseLevel(priority);
      var energyResult = ParseLevel(energy);
      var interestResult = ParseLevel(interest);

      var command = new CreateToDoCommand(
        personId,
        new Description(description),
        reminder.Value,
        priorityResult.IsSuccess ? new Priority(priorityResult.Value) : null,
        energyResult.IsSuccess ? new Energy(energyResult.Value) : null,
        interestResult.IsSuccess ? new Interest(interestResult.Value) : null
      );

      var result = await mediator.Send(command, cancellationToken);

      if (result.IsFailed)
      {
        return $"Failed to create todo: {result.GetErrorMessages()}";
      }

      return reminder.Value.HasValue
        ? $"Successfully created todo '{result.Value.Description.Value}' with a reminder set for {reminder.Value.Value:yyyy-MM-dd HH:mm:ss} UTC."
        : $"Successfully created todo '{result.Value.Description.Value}'.";
    }
    catch (Exception ex)
    {
      return $"Error creating todo: {ex.Message}";
    }
  }

  [KernelFunction("update_todo")]
  [Description("Updates an existing todo's description")]
  public async Task<string> UpdateToDoAsync(
    [Description("The todo ID (GUID)")] string todoId,
    [Description("The new todo description")] string description,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var guidResult = TryParseToDoId(todoId);
      if (guidResult.IsFailed)
      {
        return $"Failed to update todo: {guidResult.GetErrorMessages()}";
      }

      var command = new UpdateToDoCommand(
        new ToDoId(guidResult.Value),
        new Description(description)
      );

      var result = await mediator.Send(command, cancellationToken);

      return result.IsFailed ? $"Failed to update todo: {result.GetErrorMessages()}" : $"Successfully updated the todo to '{description}'.";
    }
    catch (Exception ex)
    {
      return $"Error updating todo: {ex.Message}";
    }
  }

  [KernelFunction("complete_todo")]
  [Description("Marks a todo as completed")]
  public async Task<string> CompleteToDoAsync(
    [Description("The todo ID (GUID)")] string todoId,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var guidResult = TryParseToDoId(todoId);
      if (guidResult.IsFailed)
      {
        return $"Failed to complete todo: {guidResult.GetErrorMessages()}";
      }

      var command = new CompleteToDoCommand(new ToDoId(guidResult.Value));
      var result = await mediator.Send(command, cancellationToken);

      return result.IsFailed ? $"Failed to complete todo: {result.GetErrorMessages()}" : "Successfully marked the todo as completed.";
    }
    catch (Exception ex)
    {
      return $"Error completing todo: {ex.Message}";
    }
  }

  [KernelFunction("delete_todo")]
  [Description("Deletes a todo")]
  public async Task<string> DeleteToDoAsync(
    [Description("The todo ID (GUID)")] string todoId,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var guidResult = TryParseToDoId(todoId);
      if (guidResult.IsFailed)
      {
        return $"Failed to delete todo: {guidResult.GetErrorMessages()}";
      }

      var command = new DeleteToDoCommand(new ToDoId(guidResult.Value));
      var result = await mediator.Send(command, cancellationToken);

      return result.IsFailed ? $"Failed to delete todo: {result.GetErrorMessages()}" : "Successfully deleted the todo.";
    }
    catch (Exception ex)
    {
      return $"Error deleting todo: {ex.Message}";
    }
  }

  [KernelFunction("set_priority")]
  [Description("Sets the priority level for a specific todo")]
  public async Task<string> SetPriorityAsync(
    [Description("The todo ID (GUID)")] string todoId,
    [Description("Priority level. Accepts: 'blue'/'low' (0), 'green'/'medium' (1), 'yellow'/'high' (2), 'red'/'urgent' (3), or color emojis.")] string priority,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var guidResult = TryParseToDoId(todoId);
      if (guidResult.IsFailed)
      {
        return $"Failed to set priority: {guidResult.GetErrorMessages()}";
      }

      var levelResult = ParseLevel(priority);
      if (levelResult.IsFailed)
      {
        return $"Failed to set priority: {levelResult.GetErrorMessages()}";
      }

      var command = new SetToDoPriorityCommand(personId, new ToDoId(guidResult.Value), new Priority(levelResult.Value));
      var result = await mediator.Send(command, cancellationToken);

      return result.IsFailed ? $"Failed to set priority: {result.GetErrorMessages()}" : $"Successfully set priority to {LevelToEmoji(levelResult.Value)}.";
    }
    catch (Exception ex)
    {
      return $"Error setting priority: {ex.Message}";
    }
  }

  [KernelFunction("set_energy")]
  [Description("Sets the energy level for a specific todo")]
  public async Task<string> SetEnergyAsync(
    [Description("The todo ID (GUID)")] string todoId,
    [Description("Energy level. Accepts: 'blue'/'low' (0), 'green'/'medium' (1), 'yellow'/'high' (2), 'red'/'urgent' (3), or color emojis.")] string energy,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var guidResult = TryParseToDoId(todoId);
      if (guidResult.IsFailed)
      {
        return $"Failed to set energy: {guidResult.GetErrorMessages()}";
      }

      var levelResult = ParseLevel(energy);
      if (levelResult.IsFailed)
      {
        return $"Failed to set energy: {levelResult.GetErrorMessages()}";
      }

      var command = new SetToDoEnergyCommand(personId, new ToDoId(guidResult.Value), new Energy(levelResult.Value));
      var result = await mediator.Send(command, cancellationToken);

      return result.IsFailed ? $"Failed to set energy: {result.GetErrorMessages()}" : $"Successfully set energy to {LevelToEmoji(levelResult.Value)}.";
    }
    catch (Exception ex)
    {
      return $"Error setting energy: {ex.Message}";
    }
  }

  [KernelFunction("set_interest")]
  [Description("Sets the interest level for a specific todo")]
  public async Task<string> SetInterestAsync(
    [Description("The todo ID (GUID)")] string todoId,
    [Description("Interest level. Accepts: 'blue'/'low' (0), 'green'/'medium' (1), 'yellow'/'high' (2), 'red'/'urgent' (3), or color emojis.")] string interest,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var guidResult = TryParseToDoId(todoId);
      if (guidResult.IsFailed)
      {
        return $"Failed to set interest: {guidResult.GetErrorMessages()}";
      }

      var levelResult = ParseLevel(interest);
      if (levelResult.IsFailed)
      {
        return $"Failed to set interest: {levelResult.GetErrorMessages()}";
      }

      var command = new SetToDoInterestCommand(personId, new ToDoId(guidResult.Value), new Interest(levelResult.Value));
      var result = await mediator.Send(command, cancellationToken);

      return result.IsFailed ? $"Failed to set interest: {result.GetErrorMessages()}" : $"Successfully set interest to {LevelToEmoji(levelResult.Value)}.";
    }
    catch (Exception ex)
    {
      return $"Error setting interest: {ex.Message}";
    }
  }

  [KernelFunction("set_all_priority")]
  [Description("Sets the priority level for multiple todos at once. If todoIds is empty or 'all', updates all active todos.")]
  public async Task<string> SetAllPriorityAsync(
    [Description("Comma-separated list of todo IDs (GUIDs), or empty/'all' for all active todos")] string todoIds,
    [Description("Priority level. Accepts: 'blue'/'low' (0), 'green'/'medium' (1), 'yellow'/'high' (2), 'red'/'urgent' (3), or color emojis.")] string priority,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var levelResult = ParseLevel(priority);
      if (levelResult.IsFailed)
      {
        return $"Failed to set priority: {levelResult.GetErrorMessages()}";
      }

      var ids = ParseToDoIds(todoIds);
      var command = new SetAllToDosAttributeCommand(
        personId,
        ids,
        new Priority(levelResult.Value),
        null,
        null
      );

      var result = await mediator.Send(command, cancellationToken);

      if (result.IsFailed)
      {
        return $"Failed to set priority: {result.GetErrorMessages()}";
      }

      return ids.Count == 0
        ? $"Successfully set priority to {LevelToEmoji(levelResult.Value)} for all {result.Value} active todos."
        : $"Successfully set priority to {LevelToEmoji(levelResult.Value)} for {result.Value} todos.";
    }
    catch (Exception ex)
    {
      return $"Error setting priority: {ex.Message}";
    }
  }

  [KernelFunction("set_all_energy")]
  [Description("Sets the energy level for multiple todos at once. If todoIds is empty or 'all', updates all active todos.")]
  public async Task<string> SetAllEnergyAsync(
    [Description("Comma-separated list of todo IDs (GUIDs), or empty/'all' for all active todos")] string todoIds,
    [Description("Energy level. Accepts: 'blue'/'low' (0), 'green'/'medium' (1), 'yellow'/'high' (2), 'red'/'urgent' (3), or color emojis.")] string energy,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var levelResult = ParseLevel(energy);
      if (levelResult.IsFailed)
      {
        return $"Failed to set energy: {levelResult.GetErrorMessages()}";
      }

      var ids = ParseToDoIds(todoIds);
      var command = new SetAllToDosAttributeCommand(
        personId,
        ids,
        null,
        new Energy(levelResult.Value),
        null
      );

      var result = await mediator.Send(command, cancellationToken);

      if (result.IsFailed)
      {
        return $"Failed to set energy: {result.GetErrorMessages()}";
      }

      return ids.Count == 0
        ? $"Successfully set energy to {LevelToEmoji(levelResult.Value)} for all {result.Value} active todos."
        : $"Successfully set energy to {LevelToEmoji(levelResult.Value)} for {result.Value} todos.";
    }
    catch (Exception ex)
    {
      return $"Error setting energy: {ex.Message}";
    }
  }

  [KernelFunction("set_all_interest")]
  [Description("Sets the interest level for multiple todos at once. If todoIds is empty or 'all', updates all active todos.")]
  public async Task<string> SetAllInterestAsync(
    [Description("Comma-separated list of todo IDs (GUIDs), or empty/'all' for all active todos")] string todoIds,
    [Description("Interest level. Accepts: 'blue'/'low' (0), 'green'/'medium' (1), 'yellow'/'high' (2), 'red'/'urgent' (3), or color emojis.")] string interest,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var levelResult = ParseLevel(interest);
      if (levelResult.IsFailed)
      {
        return $"Failed to set interest: {levelResult.GetErrorMessages()}";
      }

      var ids = ParseToDoIds(todoIds);
      var command = new SetAllToDosAttributeCommand(
        personId,
        ids,
        null,
        null,
        new Interest(levelResult.Value)
      );

      var result = await mediator.Send(command, cancellationToken);

      if (result.IsFailed)
      {
        return $"Failed to set interest: {result.GetErrorMessages()}";
      }

      return ids.Count == 0
        ? $"Successfully set interest to {LevelToEmoji(levelResult.Value)} for all {result.Value} active todos."
        : $"Successfully set interest to {LevelToEmoji(levelResult.Value)} for {result.Value} todos.";
    }
    catch (Exception ex)
    {
      return $"Error setting interest: {ex.Message}";
    }
  }

  [KernelFunction("list_todos")]
  [Description("Lists all active todos for the current person")]
  public async Task<string> ListToDosAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var query = new GetToDosByPersonIdQuery(personId);
      var result = await mediator.Send(query, cancellationToken);

      if (result.IsFailed)
      {
        return $"Failed to retrieve todos: {result.Errors.FirstOrDefault()?.Message}";
      }

      if (!result.Value.Any())
      {
        return "You currently have no active todos.";
      }

      return FormatToDosAsTable(result.Value);
    }
    catch (Exception ex)
    {
      return $"Error retrieving todos: {ex.Message}";
    }
  }

  [KernelFunction("generate_daily_plan")]
  [Description("Generates a suggested daily task list ordered for optimal execution. Returns an AI-curated list of tasks balanced for ADHD-friendly productivity.")]
  public async Task<string> GenerateDailyPlanAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var query = new GetDailyPlanQuery(personId);
      var result = await mediator.Send(query, cancellationToken);

      if (result.IsFailed)
      {
        return $"Failed to generate daily plan: {result.Errors.FirstOrDefault()?.Message}";
      }

      var plan = result.Value;

      // Handle empty todos case
      if (plan.SuggestedTasks.Count == 0)
      {
        return plan.SelectionRationale;
      }

      // Format as a nice table
      var sb = new StringBuilder();
      sb.AppendLine("Here's your suggested daily plan:");
      sb.AppendLine();
      sb.AppendLine("| # | Task                           | P   | E   | I   |");
      sb.AppendLine("| - | ------------------------------ | --- | --- | --- |");

      for (int i = 0; i < plan.SuggestedTasks.Count; i++)
      {
        var task = plan.SuggestedTasks[i];
        var taskName = task.Description.Length > 30
          ? task.Description[..27] + "..."
          : task.Description.PadRight(30);

        var priority = LevelToEmoji(task.Priority.Value);
        var energy = LevelToEmoji(task.Energy.Value);
        var interest = LevelToEmoji(task.Interest.Value);

        sb.AppendLine($"| {i + 1} | {taskName} | {priority}  | {energy}  | {interest}  |");
      }

      sb.AppendLine();
      sb.AppendLine("P = Priority, E = Energy, I = Interest");
      sb.AppendLine();
      sb.AppendLine($"💡 {plan.SelectionRationale}");
      sb.AppendLine();
      sb.AppendLine($"📊 Showing {plan.SuggestedTasks.Count} of {plan.TotalActiveTodos} active todos");

      return sb.ToString();
    }
    catch (Exception ex)
    {
      return $"Error generating daily plan: {ex.Message}";
    }
  }

  private static Result<Guid> TryParseToDoId(string todoId)
  {
    return !Guid.TryParse(todoId, out var todoGuid)
      ? (Result<Guid>)Result.Fail("Invalid todo ID format. The ID must be a valid GUID.")
      : Result.Ok(todoGuid);
  }

  private static List<ToDoId> ParseToDoIds(string todoIds)
  {
    if (string.IsNullOrWhiteSpace(todoIds) || todoIds.Equals("all", StringComparison.OrdinalIgnoreCase))
    {
      return [];
    }

    var ids = todoIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    return ids
      .Where(id => Guid.TryParse(id, out _))
      .Select(id => new ToDoId(Guid.Parse(id)))
      .ToList();
  }

  private static Result<Level> ParseLevel(string? input)
  {
    if (string.IsNullOrWhiteSpace(input))
    {
      return Result.Fail<Level>("Level cannot be empty.");
    }

    var normalized = input.Trim().ToLowerInvariant();

    return normalized switch
    {
      // Color names
      "blue" or "low" or "minimal" => Result.Ok(Level.Blue),
      "green" or "medium" or "normal" => Result.Ok(Level.Green),
      "yellow" or "high" => Result.Ok(Level.Yellow),
      "red" or "urgent" or "critical" => Result.Ok(Level.Red),

      // Emojis
      "🔵" => Result.Ok(Level.Blue),
      "🟢" => Result.Ok(Level.Green),
      "🟡" => Result.Ok(Level.Yellow),
      "🔴" => Result.Ok(Level.Red),

      // Numbers
      "0" => Result.Ok(Level.Blue),
      "1" => Result.Ok(Level.Green),
      "2" => Result.Ok(Level.Yellow),
      "3" => Result.Ok(Level.Red),

      _ => Result.Fail<Level>($"Invalid level '{input}'. Use: blue/low (0), green/medium (1), yellow/high (2), red/urgent (3), or color emojis.")
    };
  }

  private static string LevelToEmoji(Level level) => level switch
  {
    Level.Blue => "🔵",
    Level.Green => "🟢",
    Level.Yellow => "🟡",
    Level.Red => "🔴",
    _ => "⚪"
  };

  private static string FormatToDosAsTable(IEnumerable<ToDo> todos)
  {
    var sb = new StringBuilder();
    sb.AppendLine("Here are your active todos:");
    sb.AppendLine();
    sb.AppendLine("| Task                           | P   | E   | I   | ID                                   |");
    sb.AppendLine("| ------------------------------ | --- | --- | --- | ------------------------------------ |");

    foreach (var todo in todos)
    {
      var taskName = todo.Description.Value.Length > 30
        ? todo.Description.Value[..27] + "..."
        : todo.Description.Value.PadRight(30);

      var priority = LevelToEmoji(todo.Priority.Value);
      var energy = LevelToEmoji(todo.Energy.Value);
      var interest = LevelToEmoji(todo.Interest.Value);

      sb.AppendLine($"| {taskName} | {priority}  | {energy}  | {interest}  | {todo.Id.Value} |");
    }

    sb.AppendLine();
    sb.AppendLine("P = Priority, E = Energy, I = Interest");

    return sb.ToString();
  }

  private async Task<Result<DateTime?>> ParseReminderDateAsync(string? reminderDate, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(reminderDate))
    {
      return Result.Ok<DateTime?>(null);
    }

    // First, try to parse as fuzzy time (e.g., "in 10 minutes")
    var fuzzyResult = fuzzyTimeParser.TryParseFuzzyTime(reminderDate, timeProvider.GetUtcNow().UtcDateTime);
    if (fuzzyResult.IsSuccess)
    {
      return Result.Ok<DateTime?>(fuzzyResult.Value);
    }

    // Fall back to ISO 8601 parsing
    return !DateTime.TryParse(reminderDate, out var parsedDate)
      ? Result.Fail<DateTime?>("Invalid reminder format. Use fuzzy time like 'in 10 minutes' or ISO 8601 format like 2025-12-31T10:00:00.")
      : await ConvertToUtcAsync(parsedDate, cancellationToken);
  }

  private async Task<Result<DateTime?>> ConvertToUtcAsync(DateTime parsedDate, CancellationToken cancellationToken)
  {
    // Get user's timezone or use default
    var personResult = await personStore.GetAsync(personId, cancellationToken);
    var timeZoneId = personResult.IsSuccess && personResult.Value.TimeZoneId.HasValue
      ? personResult.Value.TimeZoneId.Value.Value
      : personConfig.DefaultTimeZoneId;

    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

    // Convert from user's local time to UTC
    var reminder = parsedDate.Kind switch
    {
      DateTimeKind.Unspecified => TimeZoneInfo.ConvertTimeToUtc(parsedDate, timeZoneInfo),
      DateTimeKind.Local => parsedDate.ToUniversalTime(),
      _ => parsedDate
    };

    return Result.Ok<DateTime?>(reminder);
  }
}
