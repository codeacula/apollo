using Apollo.Core.API;
using Apollo.Core.Conversations;
using Apollo.Core.ToDos.Requests;
using Apollo.Domain.ToDos.Models;
using Apollo.GRPC.Interceptors;
using Apollo.GRPC.Service;

using FluentResults;

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

  public async Task<Result<ToDo>> CreateToDoAsync(CreateToDoRequest request)
  {
    var grpcResult = await ApolloGrpcService.CreateToDoAsync(request);

    return grpcResult.IsSuccess ?
      Result.Ok(grpcResult.Data ?? string.Empty) :
      Result.Fail(string.Join("; ", grpcResult.Errors.Select(e => e.Message)));
  }

  public async Task<Result<string>> SendMessageAsync(NewMessageRequest request)
  {
    var grpcResult = await ApolloGrpcService.SendApolloMessageAsync(request);

    return grpcResult.IsSuccess ?
      Result.Ok(grpcResult.Data ?? string.Empty) :
      Result.Fail(string.Join("; ", grpcResult.Errors.Select(e => e.Message)));
  }
}
