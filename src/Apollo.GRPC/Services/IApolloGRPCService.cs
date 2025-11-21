using ProtoBuf.Grpc.Configuration;

namespace Apollo.GRPC.Services;

[ServiceContract]
public interface IApolloGRPCService
{
  [OperationContract]
  Task<string> SendApolloMessageAsync(string message);
}
