using Apollo.Application.Conversations;
using Apollo.Core.Conversations;
using Apollo.GRPC.Contracts;

using MediatR;

namespace Apollo.GRPC.Service;

public sealed class ApolloGrpcService(IMediator mediator) : IApolloGrpcService
{
  public async Task<GrpcResult<string>> SendApolloMessageAsync(NewMessage message)
  {
    var requestResult = await mediator.Send(new ProcessIncomingMessageCommmand(message));
    return requestResult.IsSuccess ?
      requestResult.Value :
      requestResult.Errors.Select(e => new GrpcError(e.Message)).ToArray();
  }
}
