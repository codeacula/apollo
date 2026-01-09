namespace Apollo.Database.People.Events;

public sealed record PersonTimeZoneUpdatedEvent(string PersonId, string TimeZoneId, DateTime UpdatedOn);
