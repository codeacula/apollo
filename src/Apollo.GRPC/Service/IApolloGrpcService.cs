using Apollo.GRPC.Contracts;

using ProtoBuf.Grpc.Configuration;

namespace Apollo.GRPC.Service;

[Service]
public interface IApolloGrpcService
{
  Task<GrpcResult<string>> SendApolloMessageAsync(string message);
}
