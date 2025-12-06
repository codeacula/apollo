namespace Apollo.Database.People.Events;

public sealed record AccessRevokedEvent(Guid Id, DateTime RevokedOn);
