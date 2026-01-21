using Microsoft.SemanticKernel;

namespace Apollo.AI.Requests;

/// <summary>
/// A filter that captures function invocation results for tracking tool calls.
/// </summary>
internal sealed class FunctionInvocationFilter(List<ToolCallResult> toolCalls) : IFunctionInvocationFilter
{
  public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
  {
    await next(context);

    toolCalls.Add(new ToolCallResult
    {
      PluginName = context.Function.PluginName ?? "Unknown",
      FunctionName = context.Function.Name,
      Result = context.Result?.ToString() ?? "",
      Success = true
    });
  }
}
