using Apollo.Core.Conversations;

using FluentResults;

namespace Apollo.Application.Conversations;

public sealed record ProcessIncomingMessageCommmand(NewMessage Message) : IRequest<Result<string>>;
