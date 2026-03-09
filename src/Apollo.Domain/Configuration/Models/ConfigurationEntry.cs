namespace Apollo.Domain.Configuration.Models;

public sealed record ConfigurationEntry
{
  public required string Key { get; init; }
  public required string Value { get; init; }
  public DateTime UpdatedOn { get; init; }
}
