using Apollo.Application.Configuration.Notifications;
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

public sealed class UpdateSuperAdminConfigurationCommandHandler(IConfigurationStore configurationStore, IMediator mediator)
  : IRequestHandler<UpdateSuperAdminConfigurationCommand, Result<ConfigurationData>>
{
  public async Task<Result<ConfigurationData>> Handle(UpdateSuperAdminConfigurationCommand request, CancellationToken cancellationToken = default)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.DiscordUserId))
      {
        return Result.Fail<ConfigurationData>("DiscordUserId must be provided.");
      }

      var result = await configurationStore.UpdateSuperAdminAsync(request.DiscordUserId, cancellationToken);
      if (result.IsSuccess)
      {
        await mediator.Publish(new SuperAdminConfigurationUpdatedNotification(), cancellationToken);
      }

      return result;
    }
    catch (Exception ex)
    {
      return Result.Fail<ConfigurationData>(ex.Message);
    }
  }
}
