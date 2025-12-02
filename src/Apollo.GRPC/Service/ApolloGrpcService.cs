using Apollo.Application.Commands;

using FluentResults;

using MediatR;

namespace Apollo.GRPC.Service;

public sealed class ApolloGrpcService(IMediator mediator) : IApolloGrpcService
{
  public async Task<Result<string>> SendApolloMessageAsync(string message)
  {
    return await mediator.Send(new ProcessIncomingMessage(message));
  }
}
