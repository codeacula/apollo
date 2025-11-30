using System.ServiceModel;

using Apollo.GRPC.Actions;

namespace Apollo.GRPC.Service;

[ServiceContract]
public interface IApolloGrpcService
{
  [OperationContract]
  Task<GrpcResult<string>> SendApolloMessageAsync(string message);
}
