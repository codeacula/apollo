using Apollo.Core.Configuration;

using FluentResults;

namespace Apollo.Application.Configuration;

/// <summary>
/// Designates the super admin by Discord user ID.
/// Persists via event-based store (immutable event appended to configuration stream).
/// </summary>
/// <param name="DiscordUserId"></param>
public sealed record UpdateSuperAdminConfigurationCommand(
  string? DiscordUserId
) : IRequest<Result<ConfigurationData>>;

public sealed class UpdateSuperAdminConfigurationCommandHandler(IConfigurationStore configurationStore)
  : IRequestHandler<UpdateSuperAdminConfigurationCommand, Result<ConfigurationData>>
{
  public async Task<Result<ConfigurationData>> Handle(UpdateSuperAdminConfigurationCommand request, CancellationToken cancellationToken = default)
  {
    try
    {
      return string.IsNullOrWhiteSpace(request.DiscordUserId)
        ? Result.Fail<ConfigurationData>("DiscordUserId must be provided.")
        : await configurationStore.UpdateSuperAdminAsync(request.DiscordUserId, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail<ConfigurationData>(ex.Message);
    }
  }
}
