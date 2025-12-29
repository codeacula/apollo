namespace Apollo.Database.Configuration.Events;

public sealed record ConfigurationCreatedEvent(string Key, string SystemPrompt, DateTime CreatedOn);
