using Apollo.Application.Commands;
using Apollo.GRPC.Contracts;

using MediatR;

namespace Apollo.GRPC.Service;

public class ApolloGrpcService(IMediator mediator) : IApolloGrpcService
{
  public async Task<GrpcResult<string>> SendApolloMessageAsync(string message)
  {
    return await mediator.Send(new ProcessIncomingMessage(message));
  }
}
