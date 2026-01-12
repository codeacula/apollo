namespace Apollo.Database.People.Events;

public sealed record PersonUpdatedEvent(Guid PersonId, string DisplayName, DateTime UpdatedOn);
