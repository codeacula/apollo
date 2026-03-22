using Apollo.Core.Configuration;
using Apollo.Database.Configuration.Events;

using FluentResults;

using Marten;

namespace Apollo.Database.Configuration;

public sealed class ConfigurationStore(IDocumentSession session) : IConfigurationStore
{
  public async Task<Result<ConfigurationData>> GetAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var config = await session.LoadAsync<DbConfiguration>(ConfigurationId.Root, cancellationToken);
      return config is null
        ? Result.Fail<ConfigurationData>("Configuration not initialized")
        : Result.Ok(ToConfigurationData(config));
    }
    catch (Exception ex)
    {
      return Result.Fail<ConfigurationData>(ex.Message);
    }
  }

  public async Task<Result<ConfigurationData>> UpdateAiAsync(string? modelId, string? endpoint, string? apiKey, CancellationToken cancellationToken = default)
  {
    try
    {
      var ev = new AiConfigurationUpdatedEvent(modelId, endpoint, apiKey);

      var existing = await session.LoadAsync<DbConfiguration>(ConfigurationId.Root, cancellationToken);

      if (existing is null)
      {
        session.Events.StartStream<DbConfiguration>(ConfigurationId.Root, ev);
      }
      else
      {
        session.Events.Append(ConfigurationId.Root, ev);
      }

      await session.SaveChangesAsync(cancellationToken);

      var updated = await session.Events.AggregateStreamAsync<DbConfiguration>(ConfigurationId.Root, token: cancellationToken);

      return updated is null
        ? Result.Fail<ConfigurationData>("Failed to retrieve updated configuration")
        : Result.Ok(ToConfigurationData(updated));
    }
    catch (Exception ex)
    {
      return Result.Fail<ConfigurationData>(ex.Message);
    }
  }

  public async Task<Result<ConfigurationData>> UpdateDiscordAsync(string? token, string? publicKey, string? botName, CancellationToken cancellationToken = default)
  {
    try
    {
      var ev = new DiscordConfigurationUpdatedEvent(token, publicKey, botName);

      var existing = await session.LoadAsync<DbConfiguration>(ConfigurationId.Root, cancellationToken);

      if (existing is null)
      {
        session.Events.StartStream<DbConfiguration>(ConfigurationId.Root, ev);
      }
      else
      {
        session.Events.Append(ConfigurationId.Root, ev);
      }

      await session.SaveChangesAsync(cancellationToken);

      var updated = await session.Events.AggregateStreamAsync<DbConfiguration>(ConfigurationId.Root, token: cancellationToken);

      return updated is null
        ? Result.Fail<ConfigurationData>("Failed to retrieve updated configuration")
        : Result.Ok(ToConfigurationData(updated));
    }
    catch (Exception ex)
    {
      return Result.Fail<ConfigurationData>(ex.Message);
    }
  }

  public async Task<Result<ConfigurationData>> UpdateSuperAdminAsync(string? discordUserId, CancellationToken cancellationToken = default)
  {
    try
    {
      var ev = new SuperAdminConfigurationUpdatedEvent(discordUserId);

      var existing = await session.LoadAsync<DbConfiguration>(ConfigurationId.Root, cancellationToken);

      if (existing is null)
      {
        session.Events.StartStream<DbConfiguration>(ConfigurationId.Root, ev);
      }
      else
      {
        session.Events.Append(ConfigurationId.Root, ev);
      }

      await session.SaveChangesAsync(cancellationToken);

      var updated = await session.Events.AggregateStreamAsync<DbConfiguration>(ConfigurationId.Root, token: cancellationToken);

      return updated is null
        ? Result.Fail<ConfigurationData>("Failed to retrieve updated configuration")
        : Result.Ok(ToConfigurationData(updated));
    }
    catch (Exception ex)
    {
      return Result.Fail<ConfigurationData>(ex.Message);
    }
  }

  public async Task<Result<bool>> IsInitializedAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var config = await session.LoadAsync<DbConfiguration>(ConfigurationId.Root, cancellationToken);
      return Result.Ok(config is not null);
    }
    catch (Exception ex)
    {
      return Result.Fail<bool>(ex.Message);
    }
  }

  private static ConfigurationData ToConfigurationData(DbConfiguration config) => new()
  {
    Id = config.Id,
    AiModelId = config.AiModelId,
    AiEndpoint = config.AiEndpoint,
    AiApiKey = config.AiApiKey,
    DiscordToken = config.DiscordToken,
    DiscordPublicKey = config.DiscordPublicKey,
    DiscordBotName = config.DiscordBotName,
    SuperAdminDiscordUserId = config.SuperAdminDiscordUserId,
    DefaultTimeZoneId = config.DefaultTimeZoneId,
    DefaultDailyTaskCount = config.DefaultDailyTaskCount,
  };
}
