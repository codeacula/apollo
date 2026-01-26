namespace Apollo.Database.People.Events;

public sealed record PersonUpdatedEvent(string DisplayName, DateTime UpdatedOn) : BaseEvent;
