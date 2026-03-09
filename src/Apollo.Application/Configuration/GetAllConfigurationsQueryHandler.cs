using Apollo.Core.Configuration;
using Apollo.Domain.Configuration.Models;
using FluentResults;
using MediatR;

namespace Apollo.Application.Configuration;

public class GetAllConfigurationsQueryHandler(IConfigurationStore store) : IRequestHandler<GetAllConfigurationsQuery, Result<IEnumerable<ConfigurationEntry>>>
{
  public async Task<Result<IEnumerable<ConfigurationEntry>>> Handle(GetAllConfigurationsQuery request, CancellationToken cancellationToken)
  {
    return await store.GetAllConfigurationsAsync(cancellationToken);
  }
}
