namespace Apollo.GRPC.Services;

public interface IGrpcClient
{
  IAiGrpcService AiGrpcService { get; }
  IApolloGRPCService ApolloGrpcService { get; }
}
