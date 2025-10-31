using Apollo.Core.Exceptions;

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
    _ = services.AddTransient<IApolloDbContext, ApolloDbContext>();

    return services;
  }

  public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
  {
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IApolloDbContext>();
    await dbContext.MigrateAsync(cancellationToken);
  }
}
