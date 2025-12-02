namespace Apollo.Application.Commands;

public record ProcessIncomingMessage(string Message) : IRequest<string>;
