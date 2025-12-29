namespace Apollo.Database.Configuration.Events;

public sealed record SystemPromptUpdatedEvent(string Key, string SystemPrompt, DateTime UpdatedOn);
