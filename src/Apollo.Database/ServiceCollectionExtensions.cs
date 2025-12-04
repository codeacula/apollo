using Apollo.Core.Exceptions;
using Apollo.Core.Infrastructure.Database.Stores;
using Apollo.Database.Stores;

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
    _ = services.AddScoped<IApolloDbContext, ApolloDbContext>();

    // Configure Marten for event sourcing and document storage
    _ = services.AddMarten(options =>
    {
      options.Connection(connectionString);
      options.Events.StreamIdentity = Marten.Events.StreamIdentity.AsGuid;
      // _ = options.Projections.Snapshot<UserReadModel>(Marten.Events.Projections.SnapshotLifecycle.Inline);
    })
    .UseLightweightSessions(); // Use lightweight sessions by default for better performance

    _ = services.AddScoped<IApolloUserStore, ApolloUserStore>();

    return services;
  }

  public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
  {
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IApolloDbContext>();
    await dbContext.MigrateAsync(cancellationToken);
  }
}
