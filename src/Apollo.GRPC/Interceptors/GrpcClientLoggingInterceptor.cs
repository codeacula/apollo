using Grpc.Core;
using Grpc.Core.Interceptors;

using Microsoft.Extensions.Logging;

namespace Apollo.GRPC.Interceptors;

public sealed class GrpcClientLoggingInterceptor(ILogger<GrpcClientLoggingInterceptor> logger) : Interceptor
{
  private readonly ILogger _logger = logger;

  public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
      TRequest request,
      ClientInterceptorContext<TRequest, TResponse> context,
      AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
  {
    GrpcLogger.LogStartingCall(_logger, context.Host ?? string.Empty, context.Method.Type.ToString(), context.Method.Name);
    try
    {
      var response = continuation(request, context);
      GrpcLogger.LogCallSucceeded(_logger, context.Host ?? string.Empty, context.Method.Type.ToString(), context.Method.Name, response);
      return response;
    }
    catch (Exception ex)
    {
      GrpcLogger.LogCallFailed(_logger, context.Host ?? string.Empty, context.Method.Type.ToString(), context.Method.Name, ex);
      throw;
    }
  }
}
