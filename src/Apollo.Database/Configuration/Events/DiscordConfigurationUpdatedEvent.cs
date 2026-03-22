namespace Apollo.Database.Configuration.Events;

public sealed record DiscordConfigurationUpdatedEvent(
  string? DiscordToken,
  string? DiscordPublicKey,
  string? DiscordBotName);
