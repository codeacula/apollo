using Apollo.Core.Infrastructure.API;
using Apollo.GRPC.Interceptors;
using Apollo.GRPC.Service;

using Grpc.Core.Interceptors;
using Grpc.Net.Client;

using ProtoBuf.Grpc.Client;

namespace Apollo.GRPC.Client;

public class ApolloGrpcClient : IApolloGrpcClient, IApolloAPIClient, IDisposable
{
  public IApolloGrpcService ApolloGrpcService { get; }
  private readonly GrpcChannel _channel;

  public ApolloGrpcClient(GrpcChannel channel, GrpcClientLoggingInterceptor GrpcClientLoggingInterceptor, GrpcHostConfig grpcHostConfig)
  {
    _channel = channel;
    var invoker = _channel.Intercept(GrpcClientLoggingInterceptor)
      .Intercept(metadata =>
      {
        metadata.Add("X-API-Token", grpcHostConfig.ApiToken);
        return metadata;
      });
    ApolloGrpcService = invoker.CreateGrpcService<IApolloGrpcService>();
  }

  public void Dispose()
  {
    _channel.Dispose();
    GC.SuppressFinalize(this);
  }

  public async Task<ApiResponse<string>> SendMessageAsync(string message)
  {
    var grpcResult = await ApolloGrpcService.SendApolloMessageAsync(message);

    return !grpcResult.IsSuccess
      ? new ApiResponse<string>(new APIError("100", string.Join(", ", grpcResult.Errors)))
      : new ApiResponse<string>(grpcResult.Value);
  }
}
