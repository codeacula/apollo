using Apollo.Core.API;
using Apollo.GRPC.Client;
using Apollo.GRPC.Interceptors;

using Grpc.Net.Client;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ProtoBuf.Grpc.Client;
using ProtoBuf.Grpc.Server;

namespace Apollo.GRPC;

public static class ServiceCollectionExtension
{
  public static IServiceCollection AddGrpcClientServices(this IServiceCollection services)
  {
    _ = services
      .AddSingleton(services =>
      {
        var config = services.GetRequiredService<IConfiguration>();
        return config.GetSection(nameof(GrpcHostConfig)).Get<GrpcHostConfig>() ?? throw new InvalidOperationException(
          "The configuration section for GrpcHostConfig is missing."
        );
      })
      .AddSingleton<GrpcClientLoggingInterceptor>()
      .AddSingleton(services =>
        {
          var options = services.GetRequiredService<GrpcHostConfig>();
          var loggerFactory = services.GetRequiredService<ILoggerFactory>();

          // Enable HTTP/2 without TLS when using plain HTTP
          GrpcClientFactory.AllowUnencryptedHttp2 = true;

          // Create channel options
          var channelOptions = new GrpcChannelOptions
          {
            LoggerFactory = loggerFactory
          };

          // Construct the appropriate URI based on config
          var scheme = options.UseHttps ? "https" : "http";
          var address = $"{scheme}://{options.Host}:{options.Port}";
          Console.WriteLine($"Creating gRPC channel with address: {address}");

          return GrpcChannel.ForAddress(address, channelOptions);
        });

    _ = services.AddSingleton<IApolloServiceClient, ApolloGrpcClient>();

    return services;
  }

  public static IServiceCollection AddGrpcServerServices(this IServiceCollection services)
  {
    _ = services
      .AddCodeFirstGrpc(config => config.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal);

    return services;
  }
}
