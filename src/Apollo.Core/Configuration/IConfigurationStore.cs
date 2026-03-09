using Apollo.Domain.Configuration.Models;
using FluentResults;

namespace Apollo.Core.Configuration;

public interface IConfigurationStore
{
  Task<Result<ConfigurationEntry>> GetConfigurationAsync(string key, CancellationToken cancellationToken = default);
  Task<Result<IEnumerable<ConfigurationEntry>>> GetAllConfigurationsAsync(CancellationToken cancellationToken = default);
  Task<Result> SetConfigurationAsync(string key, string value, CancellationToken cancellationToken = default);
  Task<Result> DeleteConfigurationAsync(string key, CancellationToken cancellationToken = default);
}
