namespace Apollo.Database.Users;

public sealed record UserCreatedEvent(Guid UserId, string Username, DateTime CreatedAt);

public sealed record UserGrantedAccessEvent(Guid UserId, DateTime GrantedAt);
