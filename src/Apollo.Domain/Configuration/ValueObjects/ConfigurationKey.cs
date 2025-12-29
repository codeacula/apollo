namespace Apollo.Domain.Configuration.ValueObjects;

public readonly record struct ConfigurationKey(string Value)
{
  public bool IsValid => !string.IsNullOrWhiteSpace(Value);
}
