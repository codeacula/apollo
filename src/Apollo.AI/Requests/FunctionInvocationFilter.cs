using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Apollo.Core.Logging;
using Apollo.AI.Tooling;

namespace Apollo.AI.Requests;

internal sealed class FunctionInvocationFilter(List<ToolCallResult> toolCalls, int maxToolCalls, ILogger logger) : IFunctionInvocationFilter
{
  private bool _createdToDo;
  private bool _limitReached;
  private string? _lastToolCall;
  private int _consecutiveRepeats;

  public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
  {
    var pluginName = context.Function.PluginName ?? "Unknown";
    var functionName = context.Function.Name;
    var toolCallKey = $"{pluginName}.{functionName}";

    AILogs.ToolInvocationAttempt(logger, pluginName, functionName);

    // Detect infinite loops - if the same tool is called repeatedly, STOP HARD
    if (_lastToolCall == toolCallKey)
    {
      _consecutiveRepeats++;
      if (_consecutiveRepeats >= ToolCallMatchers.MaxConsecutiveRepeats)
      {
        AILogs.InfiniteLoopDetected(logger, toolCallKey, _consecutiveRepeats + 1);
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
      AILogs.ToolInvocationBlocked(logger, pluginName, functionName, "Tool call limit reached");
      BlockInvocation(context, "Tool call limit reached for this request.", includeInResults: false);
      return;
    }

    if (_createdToDo && IsBlockedAfterCreate(context))
    {
      AILogs.ToolInvocationBlocked(logger, pluginName, functionName, "Cannot modify newly created ToDo");
      BlockInvocation(context, "Cannot complete or delete a newly created ToDo, or unlink/delete its reminders, within the same request.");
      return;
    }

    var beforeExec = DateTimeOffset.UtcNow;
    await next(context);
    var afterExec = DateTimeOffset.UtcNow;

    var result = context.Result?.ToString() ?? "";

    AILogs.ToolInvocationCompleted(logger, pluginName, functionName, (afterExec - beforeExec).TotalMilliseconds);

    toolCalls.Add(new ToolCallResult
    {
      PluginName = pluginName,
      FunctionName = functionName,
      Result = result,
      Success = true
    });

    if (ToolCallMatchers.IsCreateToDo(context.Function.PluginName, context.Function.Name))
    {
      _createdToDo = true;
    }
  }

  private static bool IsBlockedAfterCreate(FunctionInvocationContext context)
  {
    var pluginName = context.Function.PluginName;
    var functionName = context.Function.Name;
    return ToolCallMatchers.IsBlockedAfterCreateToDo(pluginName, functionName)
      || ToolCallMatchers.IsBlockedAfterCreateReminder(pluginName, functionName);
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
