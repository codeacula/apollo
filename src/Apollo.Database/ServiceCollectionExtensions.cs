using Apollo.Core.Conversations;
using Apollo.Core.Data;
using Apollo.Core.People;
using Apollo.Database.Conversations;
using Apollo.Database.Conversations.Events;
using Apollo.Database.People;
using Apollo.Database.People.Events;

using Marten;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using DbPerson = Apollo.Database.People.DbPerson;

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
      .AddMarten(options =>
      {
        options.Connection(connectionString);

        _ = options.Schema.For<DbPerson>()
          .Identity(x => x.Id)
          .UniqueIndex(x => x.Username);

        _ = options.Events.AddEventType<PersonCreatedEvent>();
        _ = options.Events.AddEventType<AccessGrantedEvent>();
        _ = options.Events.AddEventType<AccessRevokedEvent>();
        _ = options.Events.AddEventType<PersonUpdatedEvent>();

        _ = options.Schema.For<DbConversation>()
          .Identity(x => x.Id);

        _ = options.Events.AddEventType<ConversationStartedEvent>();
        _ = options.Events.AddEventType<UserSentMessageEvent>();
      })
      .UseLightweightSessions();

    _ = services
      .AddScoped<IConversationStore, ConversationStore>()
      .AddScoped<IPersonStore, PersonStore>();

    return services;
  }

  public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
  {
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IApolloDbContext>();
    await dbContext.MigrateAsync(cancellationToken);
  }
}
