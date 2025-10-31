using Apollo.GRPC.Interceptors;
using Apollo.GRPC.Services;

using Grpc.Core.Interceptors;
using Grpc.Net.Client;

using ProtoBuf.Grpc.Client;

namespace Apollo.GRPC;

public class GrpcClient : IGrpcClient, IDisposable
{
  public IAiGrpcService AiGrpcService { get; }
  public IApolloGRPCService ApolloGrpcService { get; }
  private readonly GrpcChannel _channel;

  public GrpcClient(GrpcChannel channel, GrpcClientLoggingInterceptor GrpcClientLoggingInterceptor, GrpcHostConfig grpcHostConfig)
  {
    _channel = channel;
    var invoker = _channel.Intercept(GrpcClientLoggingInterceptor)
      .Intercept(metadata =>
      {
        metadata.Add("X-API-Token", grpcHostConfig.ApiToken);
        return metadata;
      });
    AiGrpcService = invoker.CreateGrpcService<IAiGrpcService>();
    ApolloGrpcService = invoker.CreateGrpcService<IApolloGRPCService>();
  }

  public void Dispose()
  {
    _channel.Dispose();
    GC.SuppressFinalize(this);
  }
}
