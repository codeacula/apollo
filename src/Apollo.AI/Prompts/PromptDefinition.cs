namespace Apollo.AI.Prompts;

public sealed record PromptDefinition
{
  public double Temperature { get; init; } = 0.7;
  public string SystemPrompt { get; init; } = "";
}
