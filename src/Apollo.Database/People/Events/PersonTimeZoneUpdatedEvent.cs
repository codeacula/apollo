namespace Apollo.Database.People.Events;

public sealed record PersonTimezoneUpdatedEvent(Guid PersonId, string TimeZoneId, DateTime UpdatedOn);
