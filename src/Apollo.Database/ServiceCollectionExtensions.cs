using Apollo.Core.Configuration;
using Apollo.Core.Conversations;
using Apollo.Core.Data;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Database.Configuration;
using Apollo.Database.Configuration.Events;
using Apollo.Database.Conversations;
using Apollo.Database.Conversations.Events;
using Apollo.Database.People;
using Apollo.Database.People.Events;
using Apollo.Database.ToDos;
using Apollo.Database.ToDos.Events;

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
    var personConfig = configuration.GetSection(nameof(PersonConfig)).Get<PersonConfig>() ?? new PersonConfig();

    _ = services.AddDbContextPool<ApolloDbContext>(options => options.UseNpgsql(connectionString));
    _ = services.AddSingleton(new ApolloConnectionString(connectionString));
    _ = services.AddSingleton(superAdminConfig);
    _ = services.AddSingleton(personConfig);
    _ = services.AddScoped<IApolloDbContext, ApolloDbContext>();


    _ = services
      .AddMarten(options =>
      {
        options.Connection(connectionString);

        _ = options.Schema.For<DbApolloConfiguration>()
          .Identity(x => x.Key);

        _ = options.Events.AddEventType<ConfigurationCreatedEvent>();
        _ = options.Events.AddEventType<SystemPromptUpdatedEvent>();

        _ = options.Schema.For<DbPerson>()
          .Identity(x => x.Id)
          .UniqueIndex(x => x.Username);

        _ = options.Events.AddEventType<PersonCreatedEvent>();
        _ = options.Events.AddEventType<AccessGrantedEvent>();
        _ = options.Events.AddEventType<AccessRevokedEvent>();
        _ = options.Events.AddEventType<PersonUpdatedEvent>();
        _ = options.Events.AddEventType<PersonTimeZoneUpdatedEvent>();

        _ = options.Schema.For<DbConversation>()
          .Identity(x => x.Id);

        _ = options.Events.AddEventType<ConversationStartedEvent>();
        _ = options.Events.AddEventType<UserSentMessageEvent>();
        _ = options.Events.AddEventType<ApolloRepliedEvent>();

        _ = options.Schema.For<DbToDo>()
          .Identity(x => x.Id);

        _ = options.Events.AddEventType<ToDoCreatedEvent>();
        _ = options.Events.AddEventType<ToDoUpdatedEvent>();
        _ = options.Events.AddEventType<ToDoCompletedEvent>();
        _ = options.Events.AddEventType<ToDoDeletedEvent>();
        _ = options.Events.AddEventType<ToDoReminderScheduledEvent>();
        _ = options.Events.AddEventType<ToDoReminderSetEvent>();

        _ = options.Projections.Snapshot<DbApolloConfiguration>(Marten.Events.Projections.SnapshotLifecycle.Inline);
        _ = options.Projections.Snapshot<DbPerson>(Marten.Events.Projections.SnapshotLifecycle.Inline);
        _ = options.Projections.Snapshot<DbConversation>(Marten.Events.Projections.SnapshotLifecycle.Inline);
        _ = options.Projections.Snapshot<DbToDo>(Marten.Events.Projections.SnapshotLifecycle.Inline);
      })
      .UseLightweightSessions();

    _ = services
      .AddScoped<IApolloConfigurationStore, ApolloConfigurationStore>()
      .AddScoped<IConversationStore, ConversationStore>()
      .AddScoped<IPersonStore, PersonStore>()
      .AddScoped<IToDoStore, ToDoStore>();

    return services;
  }

  public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
  {
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IApolloDbContext>();
    await dbContext.MigrateAsync(cancellationToken);
  }
}
