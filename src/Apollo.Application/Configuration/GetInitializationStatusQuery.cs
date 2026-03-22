using Apollo.Core.Configuration;

using FluentResults;

namespace Apollo.Application.Configuration;

/// <summary>
/// Returns initialization status: whether the system has been configured,
/// and which subsystems (AI, Discord, SuperAdmin) are ready.
/// </summary>
public sealed record GetInitializationStatusQuery : IRequest<Result<InitializationStatus>>;

public sealed record InitializationStatus(
  bool IsInitialized,
  bool IsAiConfigured,
  bool IsDiscordConfigured,
  bool IsSuperAdminConfigured
);

public sealed class GetInitializationStatusQueryHandler(IConfigurationStore configurationStore)
  : IRequestHandler<GetInitializationStatusQuery, Result<InitializationStatus>>
{
  public async Task<Result<InitializationStatus>> Handle(GetInitializationStatusQuery request, CancellationToken cancellationToken = default)
  {
    try
    {
      var configResult = await configurationStore.GetAsync(cancellationToken);

      // If not found (not yet initialized), return a "not initialized" status
      var config = configResult.IsSuccess ? configResult.Value : ConfigurationData.Empty();

      var status = new InitializationStatus(
        IsInitialized: config.IsInitialized,
        IsAiConfigured: config.IsAiConfigured,
        IsDiscordConfigured: config.IsDiscordConfigured,
        IsSuperAdminConfigured: config.IsSuperAdminConfigured
      );

      return Result.Ok(status);
    }
    catch (Exception ex)
    {
      return Result.Fail<InitializationStatus>(ex.Message);
    }
  }
}
