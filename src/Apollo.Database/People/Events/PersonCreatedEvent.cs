using Apollo.Domain.Common.Enums;

namespace Apollo.Database.People.Events;

public sealed record PersonCreatedEvent(string Username, Platform Platform, string PlatformUserId) : BaseEvent;
