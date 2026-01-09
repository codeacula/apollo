using Apollo.Domain.Common.Enums;

namespace Apollo.Database.People.Events;

public sealed record PersonCreatedEvent(string Id, string Username, Platform Platform, string ProviderId, DateTime CreatedOn);
