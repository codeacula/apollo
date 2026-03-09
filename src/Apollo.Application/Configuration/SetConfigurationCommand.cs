using FluentResults;
using MediatR;

namespace Apollo.Application.Configuration;

public sealed record SetConfigurationCommand(string Key, string Value) : IRequest<Result>;
