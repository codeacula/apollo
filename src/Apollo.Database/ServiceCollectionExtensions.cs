using Apollo.Core.Configuration;
using Apollo.Core.Exceptions;
using Apollo.Core.Services;
using Apollo.Database.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Apollo.Database;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
  {
    const string connectionKey = "Apollo";
    var connectionString = configuration.GetConnectionString(connectionKey) ?? throw new MissingDatabaseStringException(connectionKey);

    _ = services.AddDbContextPool<ApolloDbContext>(options => options.UseNpgsql(connectionString));
    _ = services.AddTransient<IApolloDbContext, ApolloDbContext>();
    _ = services.AddScoped<ISettingsService, SettingsService>();

    _ = services
      .AddSingleton<ISettingsProvider, SettingsProvider>()
      .AddSingleton<IOptions<ApolloSettings>, ApolloSettingsOptions>();

    return services;
  }

  public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
  {
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IApolloDbContext>();
    await dbContext.MigrateAsync(cancellationToken);
  }
}
