using Apollo.GRPC.Service;

using Microsoft.AspNetCore.Builder;

namespace Apollo.GRPC;

public static class WebApplicationExtension
{
  public static WebApplication AddGrpcServerServices(this WebApplication app)
  {
    _ = app.MapGrpcService<ApolloGrpcService>();

    return app;
  }
}
