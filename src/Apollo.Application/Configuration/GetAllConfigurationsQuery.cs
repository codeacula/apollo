using Apollo.Domain.Configuration.Models;
using FluentResults;
using MediatR;

namespace Apollo.Application.Configuration;

public sealed record GetAllConfigurationsQuery() : IRequest<Result<IEnumerable<ConfigurationEntry>>>;
