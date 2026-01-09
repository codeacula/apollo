using Apollo.Core.Conversations;
using Apollo.Core.Data;
using Apollo.Core.People;
using Apollo.Core.ToDos;
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

        _ = options.Schema.For<DbPerson>()
          .Identity(x => x.Id)
          .Index(x => x.Username)
          .Index(x => new { x.Platform, x.ProviderId }, idx => idx.IsUnique = true);

        _ = options.Events.AddEventType<PersonCreatedEvent>();
        _ = options.Events.AddEventType<AccessGrantedEvent>();
        _ = options.Events.AddEventType<AccessRevokedEvent>();
        _ = options.Events.AddEventType<PersonUpdatedEvent>();
        _ = options.Events.AddEventType<PersonTimeZoneUpdatedEvent>();

        _ = options.Schema.For<DbConversation>()
          .Identity(x => x.Id)
          .Index(x => new { x.PersonPlatform, x.PersonProviderId });

        _ = options.Events.AddEventType<ConversationStartedEvent>();
        _ = options.Events.AddEventType<UserSentMessageEvent>();
        _ = options.Events.AddEventType<ApolloRepliedEvent>();

        _ = options.Schema.For<DbToDo>()
          .Identity(x => x.Id)
          .Index(x => new { x.PersonPlatform, x.PersonProviderId });

        _ = options.Events.AddEventType<ToDoCreatedEvent>();
        _ = options.Events.AddEventType<ToDoUpdatedEvent>();
        _ = options.Events.AddEventType<ToDoCompletedEvent>();
        _ = options.Events.AddEventType<ToDoDeletedEvent>();
        _ = options.Events.AddEventType<ToDoReminderScheduledEvent>();
        _ = options.Events.AddEventType<ToDoReminderSetEvent>();

        _ = options.Schema.For<DbReminder>()
          .Identity(x => x.Id);

        _ = options.Events.AddEventType<ReminderCreatedEvent>();
        _ = options.Events.AddEventType<ReminderSentEvent>();
        _ = options.Events.AddEventType<ReminderAcknowledgedEvent>();
        _ = options.Events.AddEventType<ReminderDeletedEvent>();

        _ = options.Schema.For<DbToDoReminder>()
          .Identity(x => x.Id)
          .Index(x => x.ToDoId)
          .Index(x => x.ReminderId);

        _ = options.Events.AddEventType<ToDoReminderLinkedEvent>();
        _ = options.Events.AddEventType<ToDoReminderUnlinkedEvent>();

        _ = options.Projections.Snapshot<DbPerson>(Marten.Events.Projections.SnapshotLifecycle.Inline);
        _ = options.Projections.Snapshot<DbConversation>(Marten.Events.Projections.SnapshotLifecycle.Inline);
        _ = options.Projections.Snapshot<DbToDo>(Marten.Events.Projections.SnapshotLifecycle.Inline);
        _ = options.Projections.Snapshot<DbReminder>(Marten.Events.Projections.SnapshotLifecycle.Inline);
        _ = options.Projections.Snapshot<DbToDoReminder>(Marten.Events.Projections.SnapshotLifecycle.Inline);
      })
      .UseLightweightSessions();

    _ = services
      .AddScoped<IConversationStore, ConversationStore>()
      .AddScoped<IPersonStore, PersonStore>()
      .AddScoped<IToDoStore, ToDoStore>()
      .AddScoped<IReminderStore, ReminderStore>();

    return services;
  }

  public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
  {
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IApolloDbContext>();
    await dbContext.MigrateAsync(cancellationToken);
  }
}
