namespace Apollo.Core.People;

public sealed record PersonConfig
{
  public string DefaultTimeZoneId { get; init; } = "America/Chicago";
}
