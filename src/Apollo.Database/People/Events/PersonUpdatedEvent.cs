namespace Apollo.Database.People.Events;

public sealed record PersonUpdatedEvent(Guid Id, string DisplayName, DateTime UpdatedOn);
