using System.Net.Security;

using Apollo.GRPC.Interceptors;
using Apollo.GRPC.Services;

using Grpc.Net.Client;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ProtoBuf.Grpc.Client;

namespace Apollo.GRPC;

public static class ServiceCollectionExtension
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5359:Do Not Disable Certificate Validation", Justification = "Needed for development purposes")]
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
      .AddSingleton<IGrpcClient, GrpcClient>()
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

      // Only configure SSL options when using HTTPS
      if (options.UseHttps)
      {
        channelOptions.HttpHandler = new SocketsHttpHandler
        {
          SslOptions = new SslClientAuthenticationOptions
          {
            RemoteCertificateValidationCallback = options.ValidateSslCertificate ? null : static (_, _, _, _) => true,
          },
        };
      }

      // Construct the appropriate URI based on config
      var scheme = options.UseHttps ? "https" : "http";
      var address = $"{scheme}://{options.Host}:{options.Port}";
      Console.WriteLine($"Creating gRPC channel with address: {address}");

      return GrpcChannel.ForAddress(address, channelOptions);
    });

    return services;
  }
}
