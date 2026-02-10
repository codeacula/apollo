using System.Text.RegularExpressions;

namespace Apollo.Discord.Handlers;

public static partial class QuickCommandParser
{
  [GeneratedRegex(@"^(?:todo|task)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
  private static partial Regex ToDoPattern();

  [GeneratedRegex(@"^remind(?:er)?\s+(?:me\s+)?(?:to\s+)?(.+?)\s+(?:in|at|on)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
  private static partial Regex ReminderWithTimePattern();

  public static bool TryParseToDo(string content, out string description)
  {
    description = string.Empty;

    var match = ToDoPattern().Match(content.Trim());
    if (!match.Success)
    {
      return false;
    }

    description = match.Groups[1].Value.Trim();
    return !string.IsNullOrWhiteSpace(description);
  }

  public static bool TryParseReminder(string content, out string message, out string time)
  {
    message = string.Empty;
    time = string.Empty;

    var trimmedContent = content.Trim();

    var matchWithTime = ReminderWithTimePattern().Match(trimmedContent);
    if (matchWithTime.Success)
    {
      message = matchWithTime.Groups[1].Value.Trim();
      time = matchWithTime.Groups[2].Value.Trim();
      return !string.IsNullOrWhiteSpace(message) && !string.IsNullOrWhiteSpace(time);
    }

    return false;
  }

  public static bool IsToDoCommand(string content)
  {
    var trimmed = content.TrimStart();
    return trimmed.StartsWith("todo ", StringComparison.OrdinalIgnoreCase) ||
           trimmed.StartsWith("task ", StringComparison.OrdinalIgnoreCase);
  }

  public static bool IsReminderCommand(string content)
  {
    var trimmed = content.TrimStart();
    return trimmed.StartsWith("remind", StringComparison.OrdinalIgnoreCase);
  }
}
