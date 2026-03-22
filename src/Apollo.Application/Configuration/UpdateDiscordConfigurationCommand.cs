using Apollo.Core.Configuration;

using FluentResults;

namespace Apollo.Application.Configuration;

/// <summary>
/// Updates the Discord subsystem configuration (Token, PublicKey, BotName).
/// Persists via event-based store (immutable event appended to configuration stream).
/// </summary>
/// <param name="Token"></param>
/// <param name="PublicKey"></param>
/// <param name="BotName"></param>
public sealed record UpdateDiscordConfigurationCommand(
  string? Token,
  string? PublicKey,
  string? BotName
) : IRequest<Result<ConfigurationData>>;

public sealed class UpdateDiscordConfigurationCommandHandler(IConfigurationStore configurationStore)
  : IRequestHandler<UpdateDiscordConfigurationCommand, Result<ConfigurationData>>
{
  public async Task<Result<ConfigurationData>> Handle(UpdateDiscordConfigurationCommand request, CancellationToken cancellationToken = default)
  {
    try
    {
      return string.IsNullOrWhiteSpace(request.Token) && string.IsNullOrWhiteSpace(request.PublicKey)
        ? Result.Fail<ConfigurationData>("At least one of Token or PublicKey must be provided.")
        : await configurationStore.UpdateDiscordAsync(request.Token, request.PublicKey, request.BotName, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail<ConfigurationData>(ex.Message);
    }
  }
}
