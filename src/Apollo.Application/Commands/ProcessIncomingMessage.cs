using FluentResults;

namespace Apollo.Application.Commands;

public sealed record ProcessIncomingMessage(string Message) : IRequest<Result<string>>;
