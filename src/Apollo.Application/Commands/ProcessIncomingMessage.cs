using Apollo.Core.Conversations;

using FluentResults;

namespace Apollo.Application.Commands;

public sealed record ProcessIncomingMessage(NewMessage Message) : IRequest<Result<string>>;
