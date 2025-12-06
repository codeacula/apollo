using Apollo.Core.Data;
using Apollo.Core.People;
using Apollo.Database.People;
using Apollo.Database.People.Events;

using Marten;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Person = Apollo.Database.People.Person;

namespace Apollo.Database;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
  {
    const string connectionKey = "Apollo";
    var connectionString = configuration.GetConnectionString(connectionKey) ?? throw new MissingDatabaseStringException(connectionKey);
    var superAdminConfig = configuration.GetSection(nameof(SuperAdminConfig)).Get<SuperAdminConfig>() ?? new SuperAdminConfig();

    _ = services.AddDbContextPool<ApolloDbContext>(options => options.UseNpgsql(connectionString));
    _ = services.AddSingleton(new ApolloConnectionString(connectionString));
    _ = services.AddSingleton(superAdminConfig);
    _ = services.AddScoped<IApolloDbContext, ApolloDbContext>();


    _ = services
      .AddMarten(options => options.Connection(connectionString))
      .UseLightweightSessions();

    _ = services.AddScoped<IPersonStore, PersonStore>();

    _ = services.AddSingleton(() =>
    {
      return DocumentStore.For(opts =>
      {
        opts.Connection(connectionString);

        _ = opts.Schema.For<Person>()
          .Identity(x => x.Id)
          .UniqueIndex(x => x.Username);

        _ = opts.Events.AddEventType<PersonCreatedEvent>();
      });
    });

    return services;
  }

  public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
  {
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IApolloDbContext>();
    await dbContext.MigrateAsync(cancellationToken);
  }
}
