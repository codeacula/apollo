using Microsoft.SemanticKernel;

namespace Apollo.AI.Requests;

internal sealed class FunctionInvocationFilter(List<ToolCallResult> toolCalls, int maxToolCalls) : IFunctionInvocationFilter
{
  private const string ToDoPluginName = "ToDos";
  private const string CreateToDoFunction = "create_todo";

  private static readonly HashSet<string> BlockedAfterCreateFunctions = new(StringComparer.OrdinalIgnoreCase)
  {
    "complete_todo",
    "delete_todo"
  };

  private static readonly HashSet<string> BlockedAfterCreateReminders = new(StringComparer.OrdinalIgnoreCase)
  {
    "delete_reminder",
    "unlink_reminder"
  };

  private bool _createdToDo;
  private bool _limitReached;
  private string? _lastToolCall;
  private int _consecutiveRepeats;
  private const int MaxConsecutiveRepeats = 3;

  public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
  {
    var pluginName = context.Function.PluginName ?? "Unknown";
    var functionName = context.Function.Name;
    var toolCallKey = $"{pluginName}.{functionName}";

    Console.WriteLine($"[TOOL] Attempting to invoke: {toolCallKey}");

    // Detect infinite loops - if the same tool is called repeatedly, STOP HARD
    if (_lastToolCall == toolCallKey)
    {
      _consecutiveRepeats++;
      if (_consecutiveRepeats >= MaxConsecutiveRepeats)
      {
        Console.WriteLine($"[TOOL] TERMINATED - Detected infinite loop: {toolCallKey} called {_consecutiveRepeats + 1} times in a row");
        // Throw exception to terminate the auto-invocation loop immediately
        throw new InvalidOperationException($"Tool calling loop terminated: {toolCallKey} was called {_consecutiveRepeats + 1} times consecutively, indicating an infinite loop.");
      }
    }
    else
    {
      _consecutiveRepeats = 0;
      _lastToolCall = toolCallKey;
    }

    if (_limitReached || toolCalls.Count >= maxToolCalls)
    {
      _limitReached = true;
      Console.WriteLine($"[TOOL] BLOCKED - Tool call limit reached ({toolCalls.Count}/{maxToolCalls})");
      BlockInvocation(context, "Tool call limit reached for this request.", includeInResults: false);
      return;
    }

    if (_createdToDo && IsBlockedAfterCreate(context))
    {
      Console.WriteLine("[TOOL] BLOCKED - Cannot modify newly created ToDo");
      BlockInvocation(context, "Cannot complete or delete a newly created ToDo, or unlink/delete its reminders, within the same request.");
      return;
    }

    var beforeExec = DateTimeOffset.UtcNow;
    await next(context);
    var afterExec = DateTimeOffset.UtcNow;

    var result = context.Result?.ToString() ?? "";

    Console.WriteLine($"[TOOL] Executed {pluginName}.{functionName} in {(afterExec - beforeExec).TotalMilliseconds:F0}ms");
    Console.WriteLine($"[TOOL] Result: {(result.Length > 100 ? result[..100] + "..." : result)}");

    toolCalls.Add(new ToolCallResult
    {
      PluginName = pluginName,
      FunctionName = functionName,
      Result = result,
      Success = true
    });

    if (IsCreateToDo(context))
    {
      _createdToDo = true;
    }
  }

  private static bool IsCreateToDo(FunctionInvocationContext context)
  {
    return string.Equals(context.Function.PluginName, ToDoPluginName, StringComparison.OrdinalIgnoreCase)
      && string.Equals(context.Function.Name, CreateToDoFunction, StringComparison.OrdinalIgnoreCase);
  }

  private static bool IsBlockedAfterCreate(FunctionInvocationContext context)
  {
    return string.Equals(context.Function.PluginName, ToDoPluginName, StringComparison.OrdinalIgnoreCase)
      ? BlockedAfterCreateFunctions.Contains(context.Function.Name)
      : BlockedAfterCreateReminders.Contains(context.Function.Name);
  }

  private void BlockInvocation(FunctionInvocationContext context, string errorMessage, bool includeInResults = true)
  {
    if (includeInResults)
    {
      toolCalls.Add(new ToolCallResult
      {
        PluginName = context.Function.PluginName ?? "Unknown",
        FunctionName = context.Function.Name,
        Success = false,
        ErrorMessage = errorMessage
      });
    }

    context.Result = new FunctionResult(context.Function, errorMessage);
  }
}
