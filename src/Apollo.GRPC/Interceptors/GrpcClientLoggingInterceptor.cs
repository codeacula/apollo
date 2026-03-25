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
    GrpcLogs.LogStartingCall(_logger, context.Host ?? string.Empty, (int)context.Method.Type, context.Method.Name);
    try
    {
      var call = continuation(request, context);

      async Task<TResponse> LoggedResponseAsync()
      {
        try
        {
#pragma warning disable VSTHRD003
          var response = await call.ResponseAsync.ConfigureAwait(false);
#pragma warning restore VSTHRD003
          GrpcLogs.LogCallSucceeded(_logger, context.Host ?? string.Empty, (int)context.Method.Type, context.Method.Name);
          return response;
        }
        catch (Exception ex)
        {
          GrpcLogs.LogCallFailed(_logger, context.Host ?? string.Empty, (int)context.Method.Type, context.Method.Name, ex);
          throw;
        }
      }

      return new AsyncUnaryCall<TResponse>(
        LoggedResponseAsync(),
        call.ResponseHeadersAsync,
        call.GetStatus,
        call.GetTrailers,
        call.Dispose);
    }
    catch (Exception ex)
    {
      GrpcLogs.LogCallFailed(_logger, context.Host ?? string.Empty, (int)context.Method.Type, context.Method.Name, ex);
      throw;
    }
  }
}
