using Apollo.Core.Data;
using Apollo.Core.People;
using Apollo.Database.People;

using Marten;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.Database;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
  {
    const string connectionKey = "Apollo";
    var connectionString = configuration.GetConnectionString(connectionKey) ?? throw new MissingDatabaseStringException(connectionKey);

    _ = services.AddDbContextPool<ApolloDbContext>(options => options.UseNpgsql(connectionString));
    _ = services.AddSingleton(new ApolloConnectionString(connectionString));
    _ = services.AddScoped<IApolloDbContext, ApolloDbContext>();


    _ = services
      .AddMarten(options => options.Connection(connectionString))
      .UseLightweightSessions();

    _ = services.AddScoped<IPersonStore, PersonStore>();

    return services;
  }

  public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
  {
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IApolloDbContext>();
    await dbContext.MigrateAsync(cancellationToken);
  }
}
