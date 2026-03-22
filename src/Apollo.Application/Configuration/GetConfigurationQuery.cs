using Apollo.Core.Configuration;

using FluentResults;

namespace Apollo.Application.Configuration;

/// <summary>
/// Returns the current application configuration, or an empty default if none exists.
/// </summary>
public sealed record GetConfigurationQuery : IRequest<Result<ConfigurationData>>;

public sealed class GetConfigurationQueryHandler(IConfigurationStore configurationStore)
  : IRequestHandler<GetConfigurationQuery, Result<ConfigurationData>>
{
  public async Task<Result<ConfigurationData>> Handle(GetConfigurationQuery request, CancellationToken cancellationToken = default)
  {
    try
    {
      var result = await configurationStore.GetAsync(cancellationToken);
      // If not found (not yet initialized), return an empty default instead of failing
      return result.IsFailed ? Result.Ok(ConfigurationData.Empty()) : result;
    }
    catch (Exception ex)
    {
      return Result.Fail<ConfigurationData>(ex.Message);
    }
  }
}
