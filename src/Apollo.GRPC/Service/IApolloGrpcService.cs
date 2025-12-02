using System.ServiceModel;

using Apollo.Core.Conversations;
using Apollo.GRPC.Contracts;

namespace Apollo.GRPC.Service;

[ServiceContract]
public interface IApolloGrpcService
{
  [OperationContract]
  Task<GrpcResult<string>> SendApolloMessageAsync(NewMessage message);
}
