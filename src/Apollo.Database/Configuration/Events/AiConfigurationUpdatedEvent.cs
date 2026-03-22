namespace Apollo.Database.Configuration.Events;

public sealed record AiConfigurationUpdatedEvent(
  string? AiModelId,
  string? AiEndpoint,
  string? AiApiKey);
