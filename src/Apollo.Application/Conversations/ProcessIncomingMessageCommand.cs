using Apollo.Core.Conversations;

using FluentResults;

namespace Apollo.Application.Conversations;

public sealed record ProcessIncomingMessageCommand(NewMessage Message) : IRequest<Result<string>>;
