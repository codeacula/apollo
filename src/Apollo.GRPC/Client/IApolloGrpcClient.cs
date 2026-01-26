using Apollo.GRPC.Service;

namespace Apollo.GRPC.Client;

public interface IApolloGrpcClient
{
  IApolloGrpcService ApolloGrpcService { get; }
}
