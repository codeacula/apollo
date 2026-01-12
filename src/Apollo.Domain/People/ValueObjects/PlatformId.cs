using Apollo.Domain.Common.Enums;

namespace Apollo.Domain.People.ValueObjects;

public readonly record struct PlatformId(string Username, string PlatformUserId, Platform Platform);
