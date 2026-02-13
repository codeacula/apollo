using System.Text.RegularExpressions;

namespace Apollo.Application.Conversations;

/// <summary>
/// Small deterministic parser for quick commands (todo / remind) that can be used
/// at the application level to short-circuit processing and execute deterministic actions
/// without invoking the AI flow.
/// </summary>
public static partial class QuickCommandParser
{
  [GeneratedRegex(@"^(?:todo|task)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
  private static partial Regex ToDoPattern();

  [GeneratedRegex(@"^remind(?:er)?\s+(?:me\s+)?(?:to\s+)?(.+?)\s+(?:in|at|on)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
  private static partial Regex ReminderWithTimePattern();

  /// <summary>
  /// Attempts to parse a todo quick-command and extract the description.
  /// Example: "todo Buy milk" -> description = "Buy milk"
  /// </summary>
  /// <param name="content"></param>
  /// <param name="description"></param>
  public static bool TryParseToDo(string content, out string description)
  {
    description = string.Empty;
    if (string.IsNullOrWhiteSpace(content))
    {
      return false;
    }

    var match = ToDoPattern().Match(content.Trim());
    if (!match.Success)
    {
      return false;
    }

    description = match.Groups[1].Value.Trim();
    return !string.IsNullOrWhiteSpace(description);
  }

  /// <summary>
  /// Attempts to parse a reminder quick-command and extract the message and time fragment.
  /// Examples:
  /// - "remind take a break in 30 minutes" -> message="take a break", time="30 minutes"
  /// - "reminder me to call mom in 2 hours" -> message="call mom", time="2 hours"
  /// </summary>
  /// <param name="content"></param>
  /// <param name="message"></param>
  /// <param name="time"></param>
  public static bool TryParseReminder(string content, out string message, out string time)
  {
    message = string.Empty;
    time = string.Empty;

    if (string.IsNullOrWhiteSpace(content))
    {
      return false;
    }

    var trimmed = content.Trim();

    var matchWithTime = ReminderWithTimePattern().Match(trimmed);
    if (matchWithTime.Success)
    {
      message = matchWithTime.Groups[1].Value.Trim();
      time = matchWithTime.Groups[2].Value.Trim();
      return !string.IsNullOrWhiteSpace(message) && !string.IsNullOrWhiteSpace(time);
    }

    return false;
  }

  /// <summary>
  /// Fast-check to see if the content begins with a todo/task quick command.
  /// This is intentionally cheap (no regex) for quick routing decisions.
  /// </summary>
  /// <param name="content"></param>
  public static bool IsToDoCommand(string content)
  {
    if (string.IsNullOrEmpty(content))
    {
      return false;
    }

    var trimmed = content.TrimStart();
    return trimmed.StartsWith("todo ", StringComparison.OrdinalIgnoreCase) ||
           trimmed.StartsWith("task ", StringComparison.OrdinalIgnoreCase);
  }

  /// <summary>
  /// Fast-check to see if the content looks like a remind/reminder quick command.
  /// </summary>
  /// <param name="content"></param>
  public static bool IsReminderCommand(string content)
  {
    if (string.IsNullOrEmpty(content))
    {
      return false;
    }

    var trimmed = content.TrimStart();
    return trimmed.StartsWith("remind", StringComparison.OrdinalIgnoreCase) ||
           trimmed.StartsWith("reminder", StringComparison.OrdinalIgnoreCase);
  }
}
