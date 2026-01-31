using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.People.Models;

public sealed record Person
{
  public required PersonId Id { get; init; }
  public required PlatformId PlatformId { get; init; }
  public required Username Username { get; init; }
  public required HasAccess HasAccess { get; init; }
  public PersonTimeZoneId? TimeZoneId { get; init; }
  public DailyTaskCount? DailyTaskCount { get; init; }
  public ICollection<NotificationChannel> NotificationChannels { get; init; } = [];
  public required CreatedOn CreatedOn { get; init; }
  public required UpdatedOn UpdatedOn { get; init; }
}
