using Microsoft.SemanticKernel;

namespace Apollo.AI.Requests;

/// <summary>
/// A filter that captures function invocation results for tracking tool calls.
/// </summary>
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

  public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
  {
    if (_limitReached || toolCalls.Count >= maxToolCalls)
    {
      _limitReached = true;
      BlockInvocation(context, "Tool call limit reached for this request.", includeInResults: false);
      return;
    }

    if (_createdToDo && IsBlockedAfterCreate(context))
    {
      BlockInvocation(context, "Cannot complete or delete a newly created ToDo, or unlink/delete its reminders, within the same request.");
      return;
    }

    await next(context);

    var pluginName = context.Function.PluginName ?? "Unknown";
    var functionName = context.Function.Name;
    var result = context.Result?.ToString() ?? "";

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
    if (string.Equals(context.Function.PluginName, ToDoPluginName, StringComparison.OrdinalIgnoreCase))
    {
      return BlockedAfterCreateFunctions.Contains(context.Function.Name);
    }

    return BlockedAfterCreateReminders.Contains(context.Function.Name);
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
