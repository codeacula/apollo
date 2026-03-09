using Apollo.Core.Configuration;
using Apollo.Domain.Configuration.Models;
using FluentResults;
using MediatR;

namespace Apollo.Application.Configuration;

public class GetConfigurationQueryHandler(IConfigurationStore store) : IRequestHandler<GetConfigurationQuery, Result<ConfigurationEntry>>
{
  public async Task<Result<ConfigurationEntry>> Handle(GetConfigurationQuery request, CancellationToken cancellationToken)
  {
    return await store.GetConfigurationAsync(request.Key, cancellationToken);
  }
}
