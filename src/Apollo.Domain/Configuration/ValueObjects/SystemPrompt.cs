namespace Apollo.Domain.Configuration.ValueObjects;

public readonly record struct SystemPrompt(string Value)
{
  public bool IsValid => !string.IsNullOrWhiteSpace(Value) && Value.Length <= 10000;
}
