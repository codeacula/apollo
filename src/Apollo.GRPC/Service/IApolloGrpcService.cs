using System.ServiceModel;

using FluentResults;

namespace Apollo.GRPC.Service;

[ServiceContract]
public interface IApolloGrpcService
{
  [OperationContract]
  Task<Result<string>> SendApolloMessageAsync(string message);
}
