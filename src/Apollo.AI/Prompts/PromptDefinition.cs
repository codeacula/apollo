namespace Apollo.AI.Prompts;

public sealed record PromptDefinition
{
  public double Temperature { get; init; } = 0.1;
  public string SystemPrompt { get; init; } = "";
}
