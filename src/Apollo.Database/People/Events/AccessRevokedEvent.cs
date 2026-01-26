namespace Apollo.Database.People.Events;

public sealed record AccessRevokedEvent(DateTime RevokedOn) : BaseEvent;
