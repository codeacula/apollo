using Apollo.Core.Configuration;
using Apollo.Core.Data;
using Apollo.Core.ToDos;
using Apollo.Notifications;
using Apollo.Service.Configuration;
using Apollo.Service.Jobs;

using NetCord;
using NetCord.Rest;

using Quartz;

namespace Apollo.Service;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddRequiredServices(this IServiceCollection services, IConfiguration configuration)
  {
    // Register Redis for session management
    string redisConnectionString = configuration.GetConnectionString("Redis") ?? throw new MissingDatabaseStringException("Redis");

    _ = services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(_ =>
        StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString));

    string quartzConnectionString = configuration.GetConnectionString("Quartz") ?? throw new MissingDatabaseStringException("Quartz");
    _ = services
        .AddQuartz(q =>
        {
          q.UsePersistentStore(s =>
            {
              s.UseProperties = true;
              s.UsePostgres(options =>
                {
                  options.ConnectionString = quartzConnectionString;
                  options.TablePrefix = "QRTZ_";
                });
              s.UseSystemTextJsonSerializer();
            });

        })
        .AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);

    _ = services.AddScoped<IToDoReminderScheduler, QuartzToDoReminderScheduler>();

    _ = services.AddDataProtection();
    _ = services.AddSingleton<ISecretProtector, SecretProtector>();

    // Notifications are registered but Discord channel depends on RestClient being available.
    // We defer RestClient initialization until after DB is available.
    _ = services.AddNotifications();

    return services;
  }

  /// <summary>
  /// Called after the app is built and migrations complete to register Discord notifications
  /// with the RestClient token fetched from the database.
  /// </summary>
  public static async Task InitializeDiscordRestClientAsync(this IServiceProvider serviceProvider)
  {
    var logger = serviceProvider.GetRequiredService<ILogger<SecretProtector>>();

    try
    {
      using var scope = serviceProvider.CreateScope();
      var configStore = scope.ServiceProvider.GetRequiredService<IConfigurationStore>();

      var result = await configStore.GetConfigurationAsync(ConfigurationKeys.DiscordToken);
      if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Value.Value))
      {
        logger.LogInformation("Discord token loaded from configuration store.");
        // Token is available, but RestClient registration happens at boot time through configuration, not here.
        // This is a no-op for now; the ConfigurationSubscriber will handle updates.
        return;
      }

      logger.LogWarning("Discord token not found in configuration store.");
    }
    catch (Exception ex)
    {
      logger.LogWarning(ex, "Could not initialize Discord REST client from configuration store.");
    }
  }
}
