namespace Apollo.Core.Configuration;

/// <summary>
/// DTO representing the current application configuration.
/// Returned by IConfigurationStore so Application layer handlers
/// can work with configuration without referencing Apollo.Database.
/// </summary>
public sealed record ConfigurationData
{
  public Guid Id { get; init; }
  public string? AiModelId { get; init; }
  public string? AiEndpoint { get; init; }
  public string? AiApiKey { get; init; }
  public string? DiscordToken { get; init; }
  public string? DiscordPublicKey { get; init; }
  public string? DiscordBotName { get; init; }
  public string? SuperAdminDiscordUserId { get; init; }
  public string? DefaultTimeZoneId { get; init; }
  public int DefaultDailyTaskCount { get; init; } = 5;

  public bool IsAiConfigured => !string.IsNullOrWhiteSpace(AiModelId) && !string.IsNullOrWhiteSpace(AiEndpoint);
  public bool IsDiscordConfigured => !string.IsNullOrWhiteSpace(DiscordToken) && !string.IsNullOrWhiteSpace(DiscordPublicKey);
  public bool IsSuperAdminConfigured => !string.IsNullOrWhiteSpace(SuperAdminDiscordUserId);
  public bool IsInitialized => IsAiConfigured || IsDiscordConfigured || IsSuperAdminConfigured;

  /// <summary>Returns an empty default configuration (no DB row exists yet).</summary>
  public static ConfigurationData Empty() => new() { Id = Guid.Empty };
}
