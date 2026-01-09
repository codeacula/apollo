namespace Apollo.Database.People.Events;

public sealed record AccessRevokedEvent(string PersonId, DateTime RevokedOn);
