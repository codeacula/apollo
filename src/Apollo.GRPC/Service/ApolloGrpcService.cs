using Apollo.Application.Commands;
using Apollo.Core.Conversations;
using Apollo.GRPC.Contracts;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Apollo.GRPC.Service;

public sealed class ApolloGrpcService(ILogger<ApolloGrpcService> logger, IMediator mediator) : IApolloGrpcService
{
  public async Task<GrpcResult<string>> SendApolloMessageAsync(NewMessage message)
  {
    TempLogging.SendingMessage(logger, message.Username, message.Content);
    var requestResult = await mediator.Send(new ProcessIncomingMessage(message));
    return requestResult.IsSuccess ?
      requestResult.Value :
      requestResult.Errors.Select(e => new GrpcError(e.Message)).ToArray();
  }
}
