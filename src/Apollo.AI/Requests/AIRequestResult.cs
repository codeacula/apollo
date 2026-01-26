namespace Apollo.AI.Requests;

public sealed record AIRequestResult
{
  public bool Success { get; init; }
  public string Content { get; init; } = "";
  public List<ToolCallResult> ToolCalls { get; init; } = [];
  public string? ErrorMessage { get; init; }

  /// <summary>
  /// The number of tokens used in the prompt.
  /// </summary>
  public int? PromptTokens { get; init; }
  public int? CompletionTokens { get; init; }
  public int? TotalTokens => PromptTokens.HasValue && CompletionTokens.HasValue
    ? PromptTokens + CompletionTokens
    : null;

  public bool HasToolCalls => ToolCalls.Count > 0;

  public IEnumerable<ToolCallResult> SuccessfulToolCalls =>
    ToolCalls.Where(tc => tc.Success);

  public IEnumerable<ToolCallResult> FailedToolCalls =>
    ToolCalls.Where(tc => !tc.Success);

  public string FormatActionsSummary()
  {
    return !HasToolCalls ? "None" : string.Join("\n", ToolCalls.Select(tc => $"- {tc.ToSummary()}"));
  }

  public static AIRequestResult Failure(string errorMessage) => new()
  {
    Success = false,
    ErrorMessage = errorMessage
  };

  public static AIRequestResult SuccessWithContent(string content) => new()
  {
    Success = true,
    Content = content
  };
}
