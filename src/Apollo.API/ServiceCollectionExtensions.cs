using Apollo.API.Jobs;
using Apollo.Core.Data;

using Quartz;

namespace Apollo.API;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddAPIServices(this IServiceCollection services, IConfiguration configuration)
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

          _ = q.AddJob<ToDoReminderJob>(opts => opts.WithIdentity("ToDoReminderJob"));
          _ = q.AddTrigger(opts => opts
            .ForJob("ToDoReminderJob")
            .WithIdentity("ToDoReminderJob-trigger")
            .WithSimpleSchedule(x => x
              .WithIntervalInMinutes(15)
              .RepeatForever()));
        })
        .AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);
    return services;
  }
}
