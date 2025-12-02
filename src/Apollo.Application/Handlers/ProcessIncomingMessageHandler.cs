using Apollo.AI;
using Apollo.Application.Commands;

using FluentResults;

namespace Apollo.Application.Handlers;

public sealed class ProcessIncomingMessageHandler(IApolloAIAgent apolloAIAgent) : IRequestHandler<ProcessIncomingMessage, Result<string>>
{
  public async Task<Result<string>> Handle(ProcessIncomingMessage request, CancellationToken cancellationToken)
  {
    try
    {
      var response = await apolloAIAgent.ChatAsync("Codeacula", request.Message, cancellationToken);
      return Result.Ok(response);
    }
    catch (Exception ex)
    {
      return Result.Fail<string>(ex.Message);
    }
  }
}
