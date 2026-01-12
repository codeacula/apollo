using Apollo.Domain.Common.Enums;

namespace Apollo.Database.People.Events;

public sealed record PersonCreatedEvent(Guid PersonId, string Username, Platform Platform, string PlatformUserId, DateTime CreatedOn);
