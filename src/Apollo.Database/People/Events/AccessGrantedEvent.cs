namespace Apollo.Database.People.Events;

public sealed record AccessGrantedEvent(DateTime GrantedOn) : BaseEvent;
