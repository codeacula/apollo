
using Apollo.GRPC.Service;

using Microsoft.AspNetCore.Builder;

namespace Apollo.GRPC;

public static class WebApplicationExtension
{
  public static WebApplication AddGrpcServices(this WebApplication app)
  {
    _ = app.MapGrpcService<ApolloGrpcService>();

    return app;
  }
}
