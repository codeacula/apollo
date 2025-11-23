using Apollo.GRPC.Actions;

using ProtoBuf.Grpc.Configuration;

namespace Apollo.GRPC.Services;

[Service]
public interface IApolloGRPCService
{
  Task<GrpcResult<string>> SendApolloMessageAsync(string message);
}
