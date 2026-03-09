using Apollo.Domain.Configuration.Models;
using FluentResults;
using MediatR;

namespace Apollo.Application.Configuration;

public sealed record GetConfigurationQuery(string Key) : IRequest<Result<ConfigurationEntry>>;
