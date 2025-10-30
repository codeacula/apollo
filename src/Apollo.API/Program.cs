using Apollo.AI;
using Apollo.AI.Config;
using Apollo.API;
using Apollo.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using NetCord.Hosting.Services;

using Quartz;

try
{
  WebApplicationBuilder webAppBuilder = WebApplication.CreateBuilder(args);
  _ = webAppBuilder.Services.AddControllers();
  _ = webAppBuilder.Services.AddAiServices(webAppBuilder.Configuration);
  _ = webAppBuilder.Services.AddSingleton(sp =>
  {
    var cfg = sp.GetRequiredService<IOptions<ApolloAIConfig>>().Value;
    return new ApolloAIAgent(cfg);
  });

  // TODO: Determine an appropriate exception to throw. Not having a connection string is a configuration error.
  string connectionString = webAppBuilder.Configuration.GetConnectionString("Apollo") ?? throw new InvalidOperationException("Apollo connection string is required");

  _ = webAppBuilder.Services.AddDbContextPool<ApolloDbContext>(options => options.UseNpgsql(connectionString));

  // Register Redis for session management
  string redisConnectionString = webAppBuilder.Configuration.GetConnectionString("Redis") ?? "localhost:6379,password=apollo_redis";

  // TODO: Investigate if we can clean this up any more
  _ = webAppBuilder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(_ =>
      StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString));

  _ = webAppBuilder.Services
      .AddQuartz(q =>
      {
        q.UsePersistentStore(s =>
          {
            s.UseProperties = true;
            s.UsePostgres(options =>
              {
                options.ConnectionString = connectionString;
                options.TablePrefix = "QRTZ_";
              });
            s.UseSystemTextJsonSerializer();
          });
      })
      .AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);

  WebApplication app = webAppBuilder.Build();

  // Apply database migrations
  using (IServiceScope scope = app.Services.CreateScope())
  {
    ApolloDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApolloDbContext>();
    await dbContext.Database.MigrateAsync();
  }

  _ = app.AddModules(typeof(IApolloAPIApp).Assembly);
  // app.AddModules(typeof(Apollo.Discord.IApolloDiscord).Assembly);
  _ = app.UseRequestLocalization();

  if (app.Environment.IsDevelopment())
  {
    _ = app.MapOpenApi();
  }

  _ = app.MapControllers();

  _ = app.UseHttpsRedirection();

  _ = app.UseDefaultFiles();
  _ = app.UseStaticFiles();

  await app.RunAsync();
}
catch (Exception ex)
{
  await Console.Error.WriteLineAsync(ex.ToString());
}
