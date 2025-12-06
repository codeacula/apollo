using Apollo.Domain.Common.Enums;

namespace Apollo.Database.People.Events;

public sealed record PersonCreatedEvent(Guid Id, string Username, Platform Platform, DateTime CreatedOn);
