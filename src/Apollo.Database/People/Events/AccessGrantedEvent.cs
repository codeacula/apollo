namespace Apollo.Database.People.Events;

public sealed record AccessGrantedEvent(Guid PersonId, DateTime GrantedOn);
