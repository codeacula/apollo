namespace Apollo.Database.People.Events;

public sealed record AccessGrantedEvent(string PersonId, DateTime GrantedOn);
