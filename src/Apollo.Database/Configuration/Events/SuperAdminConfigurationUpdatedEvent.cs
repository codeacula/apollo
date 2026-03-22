namespace Apollo.Database.Configuration.Events;

public sealed record SuperAdminConfigurationUpdatedEvent(
  string? SuperAdminDiscordUserId);
