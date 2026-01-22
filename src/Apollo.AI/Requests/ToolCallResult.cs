namespace Apollo.AI.Requests;

public sealed record ToolCallResult
{
  public required string PluginName { get; init; }
  public required string FunctionName { get; init; }
  public string Arguments { get; init; } = "";
  public string Result { get; init; } = "";
  public bool Success { get; init; } = true;
  public string? ErrorMessage { get; init; }

  public string FullFunctionName => $"{PluginName}.{FunctionName}";

  public string ToSummary() => Success
    ? $"{FullFunctionName}: {Result}"
    : $"{FullFunctionName}: Failed - {ErrorMessage}";
}
