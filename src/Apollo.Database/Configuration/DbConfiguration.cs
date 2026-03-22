using Apollo.Database.Configuration.Events;

using JasperFx.Events;

namespace Apollo.Database.Configuration;

public sealed record DbConfiguration
{
  public Guid Id { get; init; } = ConfigurationId.Root;
  public string? AiModelId { get; init; }
  public string? AiEndpoint { get; init; }
  public string? AiApiKey { get; init; }
  public string? DiscordToken { get; init; }
  public string? DiscordPublicKey { get; init; }
  public string? DiscordBotName { get; init; }
  public string? SuperAdminDiscordUserId { get; init; }
  public string? DefaultTimeZoneId { get; init; }
  public int DefaultDailyTaskCount { get; init; } = 5;

  public static DbConfiguration Apply(IEvent<AiConfigurationUpdatedEvent> ev, DbConfiguration config)
  {
    return config with
    {
      AiModelId = ev.Data.AiModelId,
      AiEndpoint = ev.Data.AiEndpoint,
      AiApiKey = ev.Data.AiApiKey,
    };
  }

  public static DbConfiguration Apply(IEvent<DiscordConfigurationUpdatedEvent> ev, DbConfiguration config)
  {
    return config with
    {
      DiscordToken = ev.Data.DiscordToken,
      DiscordPublicKey = ev.Data.DiscordPublicKey,
      DiscordBotName = ev.Data.DiscordBotName,
    };
  }

  public static DbConfiguration Apply(IEvent<SuperAdminConfigurationUpdatedEvent> ev, DbConfiguration config)
  {
    return config with
    {
      SuperAdminDiscordUserId = ev.Data.SuperAdminDiscordUserId,
    };
  }
}

