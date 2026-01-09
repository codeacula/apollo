namespace Apollo.Database.People.Events;

public sealed record PersonUpdatedEvent(string PersonId, string DisplayName, DateTime UpdatedOn);
