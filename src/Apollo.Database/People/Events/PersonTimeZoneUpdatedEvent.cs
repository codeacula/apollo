namespace Apollo.Database.People.Events;

public sealed record PersonTimeZoneUpdatedEvent(Guid PersonId, string TimeZoneId, DateTime UpdatedOn);
