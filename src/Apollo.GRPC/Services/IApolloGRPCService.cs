using ProtoBuf.Grpc.Configuration;

namespace Apollo.GRPC.Services;

[Service]
public interface IApolloGRPCService
{
  Task<string> SendApolloMessageAsync(string message);
}
