using Apollo.Core.Data;
using Apollo.Core.ToDos;
using Apollo.Notifications;
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

    // Register Discord REST client for notifications
    var discordToken = configuration["Discord:Token"];
    if (!string.IsNullOrWhiteSpace(discordToken))
    {
      _ = services.AddSingleton(new RestClient(new BotToken(discordToken)));
      _ = services.AddNotificationsWithChannels();
    }
    else
    {
      _ = services.AddNotifications();
    }

    return services;
  }
}
