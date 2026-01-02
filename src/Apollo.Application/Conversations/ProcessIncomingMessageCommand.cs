using Apollo.Core.Conversations;

using FluentResults;

namespace Apollo.Application.Conversations;

/// <summary>
/// Tells the system to process an incoming message from a supported platform.
/// </summary>
/// <param name="Message">The message to process.</param>
/// <seealso cref="ProcessIncomingMessageCommandHandler"/>
public sealed record ProcessIncomingMessageCommand(NewMessageRequest Message) : IRequest<Result<Reply>>;
