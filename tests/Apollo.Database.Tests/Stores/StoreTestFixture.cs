using Apollo.Core;
using Apollo.Core.People;
using Apollo.Database.Conversations;
using Apollo.Database.Conversations.Events;
using Apollo.Database.People;
using Apollo.Database.People.Events;
using Apollo.Database.ToDos;
using Apollo.Database.ToDos.Events;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Marten;

using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace Apollo.Database.Tests.Stores;

/// <summary>
/// Test fixture that sets up an in-memory Marten document store for integration testing.
/// Provides utilities for creating test data and managing sessions.
/// </summary>
public sealed class StoreTestFixture : IAsyncLifetime
{
  private IDocumentStore? _store;
  private Mock<IPersonCache>? _personCacheMock;

  public IDocumentSession DocumentSession { get; private set; } = null!;
  public IPersonCache PersonCache => _personCacheMock!.Object;
  public TimeProvider TimeProvider => TimeProvider.System;
  public SuperAdminConfig SuperAdminConfig { get => field!; private set; }

  public async Task InitializeAsync()
  {
    _personCacheMock = new Mock<IPersonCache>();
    _ = _personCacheMock
      .Setup(c => c.MapPlatformIdToPersonIdAsync(It.IsAny<PlatformId>(), It.IsAny<PersonId>()))
      .ReturnsAsync(Result.Ok());
    _ = _personCacheMock
      .Setup(c => c.InvalidateAccessAsync(It.IsAny<PersonId>()))
      .ReturnsAsync(Result.Ok());



    SuperAdminConfig = new SuperAdminConfig();

    var services = new ServiceCollection();
    _ = services.AddMarten(options =>
    {
      options.Connection("Server=127.0.0.1;Port=5432;Database=apollo_test;User Id=postgres;Password=postgres;");

      // Configure aggregates
      _ = options.Schema.For<DbPerson>()
        .Identity(x => x.Id)
        .Index(x => x.Username)
        .Index(x => new { x.Platform, x.PlatformUserId }, idx => idx.IsUnique = true);

      _ = options.Schema.For<DbConversation>()
        .Identity(x => x.Id)
        .Index(x => x.PersonId);

      _ = options.Schema.For<DbToDo>()
        .Identity(x => x.Id)
        .Index(x => x.PersonId)
        .Index(x => x.IsDeleted);

      _ = options.Schema.For<DbReminder>()
        .Identity(x => x.Id)
        .Index(x => x.QuartzJobId)
        .Index(x => x.IsDeleted);

      _ = options.Schema.For<DbToDoReminder>()
        .Identity(x => x.Id)
        .Index(x => new { x.ToDoId, x.ReminderId })
        .Index(x => x.IsDeleted);

      // Register all event types
      RegisterPersonEvents(options);
      RegisterConversationEvents(options);
      RegisterToDoEvents(options);
      RegisterReminderEvents(options);
    })
    .UseLightweightSessions();

    var provider = services.BuildServiceProvider();
    _store = provider.GetRequiredService<IDocumentStore>();

    await _store.Advanced.Clean.DeleteAllDocumentsAsync();
    DocumentSession = _store.LightweightSession();
  }

  public async Task DisposeAsync()
  {
    DocumentSession?.Dispose();

    if (_store != null)
    {
      await _store.Advanced.Clean.DeleteAllDocumentsAsync();
      _store.Dispose();
    }
  }

  private static void RegisterPersonEvents(StoreOptions options)
  {
    _ = options.Events.AddEventType<PersonCreatedEvent>();
    _ = options.Events.AddEventType<PersonUpdatedEvent>();
    _ = options.Events.AddEventType<AccessGrantedEvent>();
    _ = options.Events.AddEventType<AccessRevokedEvent>();
    _ = options.Events.AddEventType<PersonTimeZoneUpdatedEvent>();
    _ = options.Events.AddEventType<PersonDailyTaskCountUpdatedEvent>();
    _ = options.Events.AddEventType<NotificationChannelAddedEvent>();
    _ = options.Events.AddEventType<NotificationChannelRemovedEvent>();
    _ = options.Events.AddEventType<NotificationChannelToggledEvent>();
  }

  private static void RegisterConversationEvents(StoreOptions options)
  {
    _ = options.Events.AddEventType<ConversationStartedEvent>();
    _ = options.Events.AddEventType<UserSentMessageEvent>();
    _ = options.Events.AddEventType<ApolloRepliedEvent>();
  }

  private static void RegisterToDoEvents(StoreOptions options)
  {
    _ = options.Events.AddEventType<ToDoCreatedEvent>();
    _ = options.Events.AddEventType<ToDoUpdatedEvent>();
    _ = options.Events.AddEventType<ToDoCompletedEvent>();
    _ = options.Events.AddEventType<ToDoDeletedEvent>();
    _ = options.Events.AddEventType<ToDoPriorityUpdatedEvent>();
    _ = options.Events.AddEventType<ToDoEnergyUpdatedEvent>();
    _ = options.Events.AddEventType<ToDoInterestUpdatedEvent>();
  }

  private static void RegisterReminderEvents(StoreOptions options)
  {
    _ = options.Events.AddEventType<ReminderCreatedEvent>();
    _ = options.Events.AddEventType<ReminderSentEvent>();
    _ = options.Events.AddEventType<ReminderAcknowledgedEvent>();
    _ = options.Events.AddEventType<ReminderDeletedEvent>();
    _ = options.Events.AddEventType<ToDoReminderLinkedEvent>();
    _ = options.Events.AddEventType<ToDoReminderUnlinkedEvent>();
  }

  /// <summary>
  /// Test data builders
  /// </summary>
  /// <param name="username"></param>
  /// <param name="platform"></param>
  public static PlatformId CreateTestPlatformId(string username = "testuser", Platform platform = Platform.Discord)
  {
    return new PlatformId(username, Guid.NewGuid().ToString(), platform);
  }

  public static PersonId CreateTestPersonId()
  {
    return new PersonId(Guid.NewGuid());
  }

  public static ToDoId CreateTestToDoId()
  {
    return new ToDoId(Guid.NewGuid());
  }

  public static ConversationId CreateTestConversationId()
  {
    return new ConversationId(Guid.NewGuid());
  }

  public static ReminderId CreateTestReminderId()
  {
    return new ReminderId(Guid.NewGuid());
  }

  public static Description CreateTestDescription(string value = "Test description")
  {
    return new Description(value);
  }

  public static Details CreateTestDetails(string value = "Test details")
  {
    return new Details(value);
  }

  public static Priority CreateTestPriority(Level level = Level.Yellow)
  {
    return new Priority(level);
  }

  public static Energy CreateTestEnergy(Level level = Level.Green)
  {
    return new Energy(level);
  }

  public static Interest CreateTestInterest(Level level = Level.Green)
  {
    return new Interest(level);
  }

  public static Content CreateTestContent(string value = "Test content")
  {
    return new Content(value);
  }

  public static ReminderTime CreateTestReminderTime(DateTime? time = null)
  {
    time ??= DateTime.UtcNow.AddDays(1);
    return new ReminderTime(time.Value);
  }

  public static QuartzJobId CreateTestQuartzJobId()
  {
    return new QuartzJobId(Guid.NewGuid());
  }

  public static Username CreateTestUsername(string value = "testuser")
  {
    return new Username(value);
  }

  public static PersonTimeZoneId CreateTestPersonTimeZoneId()
  {
    _ = PersonTimeZoneId.TryParse("America/New_York", out var timeZoneId, out _);
    return timeZoneId;
  }

  public static DailyTaskCount CreateTestDailyTaskCount(int value = 5)
  {
    return new DailyTaskCount(value);
  }
}
