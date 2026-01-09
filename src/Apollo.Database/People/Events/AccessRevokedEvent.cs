namespace Apollo.Database.People.Events;

public sealed record AccessRevokedEvent(Guid PersonId, DateTime RevokedOn);
