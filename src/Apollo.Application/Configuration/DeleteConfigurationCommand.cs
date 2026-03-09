using FluentResults;
using MediatR;

namespace Apollo.Application.Configuration;

public sealed record DeleteConfigurationCommand(string Key) : IRequest<Result>;
