using Apollo.Core.Configuration;
using Apollo.GRPC.Client;
using Apollo.GRPC.Contracts;

using Microsoft.Extensions.Configuration.Memory;

namespace Apollo.Discord;

/// <summary>
/// Fetches the Discord token from Apollo.Service (via gRPC) at startup
/// and injects it into the application's IConfiguration so NetCord can read it.
/// </summary>
public static class DiscordTokenInitializer
{
  public static async Task LoadTokenIntoConfigurationAsync(
    IConfigurationBuilder configBuilder,
    IApolloGrpcClient grpcClient,
    ILogger logger)
  {
    try
    {
      var result = await grpcClient.ApolloGrpcService.GetConfigurationAsync(
        new GetConfigurationRequest { Key = ConfigurationKeys.DiscordToken });

      if (result.IsSuccess && result.Data is not null)
      {
        configBuilder.Add(new MemoryConfigurationSource
        {
          InitialData = new Dictionary<string, string?>
          {
            ["Discord:Token"] = result.Data.Value,
          }
        });
        logger.LogInformation("Discord token loaded from configuration store.");
        return;
      }

      logger.LogWarning("Discord token not found in configuration store.");
    }
    catch (Exception ex)
    {
      logger.LogWarning(ex, "Could not fetch Discord token from Apollo.Service.");
    }
  }
}
