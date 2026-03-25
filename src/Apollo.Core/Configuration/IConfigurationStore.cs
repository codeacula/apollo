using FluentResults;

namespace Apollo.Core.Configuration;

public interface IConfigurationStore
{
  Task<Result<ConfigurationData>> GetAsync(CancellationToken cancellationToken = default);
  Task<Result<ConfigurationData>> UpdateAiAsync(string? modelId, string? endpoint, string? apiKey, CancellationToken cancellationToken = default);
  Task<Result<ConfigurationData>> UpdateDiscordAsync(string? token, string? publicKey, string? botName, CancellationToken cancellationToken = default);
  Task<Result<ConfigurationData>> UpdateSuperAdminAsync(string? discordUserId, CancellationToken cancellationToken = default);
  Task<Result<bool>> IsInitializedAsync(CancellationToken cancellationToken = default);
}
