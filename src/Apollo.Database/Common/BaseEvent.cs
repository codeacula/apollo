namespace Apollo.Database.Common;

public record BaseEvent
{
  public required Guid Id { get; init; }
  public required DateTime CreatedOn { get; init; }
}
