using Apollo.Core.Configuration;

using FluentResults;

namespace Apollo.Application.Configuration;

/// <summary>
/// Updates the AI subsystem configuration (ModelId, Endpoint, ApiKey).
/// Persists via event-based store (immutable event appended to configuration stream).
/// </summary>
/// <param name="ModelId"></param>
/// <param name="Endpoint"></param>
/// <param name="ApiKey"></param>
public sealed record UpdateAiConfigurationCommand(
  string? ModelId,
  string? Endpoint,
  string? ApiKey
) : IRequest<Result<ConfigurationData>>;

public sealed class UpdateAiConfigurationCommandHandler(IConfigurationStore configurationStore)
  : IRequestHandler<UpdateAiConfigurationCommand, Result<ConfigurationData>>
{
  public async Task<Result<ConfigurationData>> Handle(UpdateAiConfigurationCommand request, CancellationToken cancellationToken = default)
  {
    try
    {
      return string.IsNullOrWhiteSpace(request.ModelId) && string.IsNullOrWhiteSpace(request.Endpoint)
        ? Result.Fail<ConfigurationData>("At least one of ModelId or Endpoint must be provided.")
        : await configurationStore.UpdateAiAsync(request.ModelId, request.Endpoint, request.ApiKey, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail<ConfigurationData>(ex.Message);
    }
  }
}
