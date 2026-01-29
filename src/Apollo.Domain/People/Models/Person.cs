using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.People.Models;

public sealed record Person
{
  public PersonId Id { get; init; }
  public PlatformId PlatformId { get; init; }
  public Username Username { get; init; }
  public HasAccess HasAccess { get; init; }
  public PersonTimeZoneId? TimeZoneId { get; init; }
  public DailyTaskCount? DailyTaskCount { get; init; }
  public ICollection<NotificationChannel> NotificationChannels { get; init; } = [];
  public CreatedOn CreatedOn { get; init; }
  public UpdatedOn UpdatedOn { get; init; }
}
