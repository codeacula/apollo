using System.Reflection;

using Apollo.AI.DTOs;
using Apollo.AI.Models;
using Apollo.AI.Requests;

namespace Apollo.AI.Tooling;

public sealed class ToolPlanValidator
{
  public static ToolPlanValidationResult Validate(ToolPlan plan, ToolPlanValidationContext context)
  {
    var result = new ToolPlanValidationResult();
    if (plan.ToolCalls.Count == 0)
    {
      return result;
    }

    var allowsTimezoneChange = HasTimezoneContext(context.ConversationHistory);
    var createdToDo = false;
    var lastToolCallKey = "";
    var consecutiveRepeats = 0;

    foreach (var toolCall in plan.ToolCalls)
    {
      if (result.ApprovedCalls.Count >= ToolCallMatchers.MaxToolCalls)
      {
        result.BlockedCalls.Add(Blocked(toolCall, "Tool call limit reached for this request."));
        continue;
      }

      if (!ToolCallResolver.TryResolve(context.Plugins, toolCall.PluginName, toolCall.FunctionName, out var resolved, out var errorMessage))
      {
        result.BlockedCalls.Add(Blocked(toolCall, errorMessage));
        continue;
      }

      if (!HasRequiredArguments(resolved.Method, toolCall.Arguments, out var missingArguments))
      {
        result.BlockedCalls.Add(Blocked(toolCall, $"Missing required arguments: {string.Join(", ", missingArguments)}"));
        continue;
      }

      var toolCallKey = $"{toolCall.PluginName}.{toolCall.FunctionName}";
      if (string.Equals(toolCallKey, lastToolCallKey, StringComparison.OrdinalIgnoreCase))
      {
        consecutiveRepeats++;
        if (consecutiveRepeats >= ToolCallMatchers.MaxConsecutiveRepeats)
        {
          result.BlockedCalls.Add(Blocked(toolCall, "Repeated tool call detected; blocked to prevent loops."));
          continue;
        }
      }
      else
      {
        consecutiveRepeats = 0;
        lastToolCallKey = toolCallKey;
      }

      if (!allowsTimezoneChange && ToolCallMatchers.IsSetTimezone(toolCall.PluginName, toolCall.FunctionName))
      {
        result.BlockedCalls.Add(Blocked(toolCall, "Timezone changes require an explicit user request or timezone context."));
        continue;
      }

      if (createdToDo && IsBlockedAfterCreate(toolCall))
      {
        result.BlockedCalls.Add(Blocked(toolCall, "Cannot modify a newly created todo within the same request."));
        continue;
      }

      if (ToolCallMatchers.IsCreateToDo(toolCall.PluginName, toolCall.FunctionName))
      {
        createdToDo = true;
      }

      result.ApprovedCalls.Add(NormalizeArguments(toolCall));
    }

    return result;
  }

  private static bool HasRequiredArguments(MethodInfo method, IDictionary<string, string?> arguments, out List<string> missing)
  {
    missing = [];
    var argumentLookup = arguments is null
      ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
      : new Dictionary<string, string?>(arguments, StringComparer.OrdinalIgnoreCase);
    foreach (var parameter in method.GetParameters())
    {
      if (parameter.ParameterType == typeof(CancellationToken))
      {
        continue;
      }

      if (parameter.HasDefaultValue)
      {
        continue;
      }

      if (string.IsNullOrWhiteSpace(parameter.Name))
      {
        continue;
      }

      if (!argumentLookup.TryGetValue(parameter.Name, out var value) || string.IsNullOrWhiteSpace(value))
      {
        missing.Add(parameter.Name);
      }
    }

    return missing.Count == 0;
  }

  private static PlannedToolCall NormalizeArguments(PlannedToolCall toolCall)
  {
    var normalizedArguments = toolCall.Arguments is null
      ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
      : new Dictionary<string, string?>(toolCall.Arguments, StringComparer.OrdinalIgnoreCase);
    return toolCall with { Arguments = normalizedArguments };
  }

  private static bool IsBlockedAfterCreate(PlannedToolCall toolCall)
  {
    return ToolCallMatchers.IsBlockedAfterCreateToDo(toolCall.PluginName, toolCall.FunctionName)
      || ToolCallMatchers.IsBlockedAfterCreateReminder(toolCall.PluginName, toolCall.FunctionName);
  }

  private static bool HasTimezoneContext(IEnumerable<ChatMessageDTO> history)
  {
    return history.Any(message => ContainsTimezoneKeyword(message.Content));
  }

  private static bool ContainsTimezoneKeyword(string content)
  {
    return content.Contains("timezone", StringComparison.OrdinalIgnoreCase)
      || content.Contains("time zone", StringComparison.OrdinalIgnoreCase)
      || content.Contains("tz", StringComparison.OrdinalIgnoreCase);
  }

  private static ToolCallResult Blocked(PlannedToolCall toolCall, string errorMessage)
  {
    var arguments = toolCall.Arguments ?? [];
    return new ToolCallResult
    {
      PluginName = toolCall.PluginName,
      FunctionName = toolCall.FunctionName,
      Arguments = string.Join(", ", arguments.Select(kv => $"{kv.Key}={kv.Value}")),
      Success = false,
      ErrorMessage = errorMessage
    };
  }
}
