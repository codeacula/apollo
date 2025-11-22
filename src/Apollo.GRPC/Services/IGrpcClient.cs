namespace Apollo.GRPC.Services;

public interface IGrpcClient
{
  IApolloGRPCService ApolloGrpcService { get; }
}
