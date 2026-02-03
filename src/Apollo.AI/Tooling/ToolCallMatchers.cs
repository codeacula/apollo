namespace Apollo.AI.Tooling;

/// <summary>
/// Centralized tool call matching logic and constants used across AI components.
/// Consolidates common validation patterns to prevent duplication.
/// </summary>
public static class ToolCallMatchers
{
  /// <summary>Plugin names</summary>
  public const string ToDoPluginName = "ToDos";
  public const string PersonPluginName = "Person";
  public const string RemindersPluginName = "Reminders";

  /// <summary>Function names</summary>
  public const string CreateToDoFunction = "create_todo";
  public const string SetTimezoneFunction = "set_timezone";

  /// <summary>Constraints</summary>
  public const int MaxToolCalls = 5;
  public const int MaxConsecutiveRepeats = 3;

  /// <summary>Functions blocked after creating a ToDo</summary>
  public static readonly HashSet<string> BlockedAfterCreateToDo = new(StringComparer.OrdinalIgnoreCase)
  {
    "complete_todo",
    "delete_todo"
  };

  /// <summary>Functions blocked after creating a reminder</summary>
  public static readonly HashSet<string> BlockedAfterCreateReminder = new(StringComparer.OrdinalIgnoreCase)
  {
    "delete_reminder",
    "unlink_reminder"
  };

  /// <summary>Check if a tool call is creating a ToDo</summary>
  /// <param name="pluginName"></param>
  /// <param name="functionName"></param>
  public static bool IsCreateToDo(string? pluginName, string? functionName)
  {
    return string.Equals(pluginName, ToDoPluginName, StringComparison.OrdinalIgnoreCase)
      && string.Equals(functionName, CreateToDoFunction, StringComparison.OrdinalIgnoreCase);
  }

  /// <summary>Check if a tool call is blocked after creating a ToDo</summary>
  /// <param name="pluginName"></param>
  /// <param name="functionName"></param>
  public static bool IsBlockedAfterCreateToDo(string? pluginName, string? functionName)
  {
    return string.Equals(pluginName, ToDoPluginName, StringComparison.OrdinalIgnoreCase)
      && BlockedAfterCreateToDo.Contains(functionName ?? "");
  }

  /// <summary>Check if a tool call is blocked after creating a reminder</summary>
  /// <param name="pluginName"></param>
  /// <param name="functionName"></param>
  public static bool IsBlockedAfterCreateReminder(string? pluginName, string? functionName)
  {
    return string.Equals(pluginName, RemindersPluginName, StringComparison.OrdinalIgnoreCase)
      && BlockedAfterCreateReminder.Contains(functionName ?? "");
  }

  /// <summary>Check if a tool call is setting the timezone</summary>
  /// <param name="pluginName"></param>
  /// <param name="functionName"></param>
  public static bool IsSetTimezone(string? pluginName, string? functionName)
  {
    return string.Equals(pluginName, PersonPluginName, StringComparison.OrdinalIgnoreCase)
      && string.Equals(functionName, SetTimezoneFunction, StringComparison.OrdinalIgnoreCase);
  }
}
