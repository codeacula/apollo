using Apollo.AI;
using Apollo.Application.Commands;

namespace Apollo.Application.Handlers;

public class ProcessIncomingMessageHandler(IApolloAIAgent apolloAIAgent) : IRequestHandler<ProcessIncomingMessage, string>
{
  public async Task<string> Handle(ProcessIncomingMessage request, CancellationToken cancellationToken)
  {
    return await apolloAIAgent.ChatAsync("Codeacula", request.Message);
  }
}
