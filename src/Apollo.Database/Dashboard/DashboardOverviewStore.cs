using Apollo.Core.Dashboard;
using Apollo.Database.Conversations;
using Apollo.Database.People;
using Apollo.Database.ToDos;

using Marten;

namespace Apollo.Database.Dashboard;

public sealed class DashboardOverviewStore(IQuerySession querySession) : IDashboardOverviewStore
{
  public async Task<DashboardOverviewData> GetOverviewDataAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
  {
    var dayStart = nowUtc.Date;
    var messagesWindowStart = nowUtc.AddHours(-24);
    var remindersWindowEnd = nowUtc.AddHours(24);

    var totalPeople = await querySession.Query<DbPerson>().CountAsync(cancellationToken);
    var peopleWithAccess = await querySession.Query<DbPerson>().CountAsync(x => x.HasAccess, token: cancellationToken);

    var activeToDos = await querySession.Query<DbToDo>()
      .Where(x => !x.IsDeleted && !x.IsCompleted)
      .CountAsync(cancellationToken);
    var completedToDos = await querySession.Query<DbToDo>()
      .Where(x => !x.IsDeleted && x.IsCompleted)
      .CountAsync(cancellationToken);
    var createdTodayToDos = await querySession.Query<DbToDo>()
      .Where(x => !x.IsDeleted && x.CreatedOn >= dayStart)
      .CountAsync(cancellationToken);

    var scheduledReminders = await querySession.Query<DbReminder>()
      .Where(x => !x.IsDeleted)
      .CountAsync(cancellationToken);
    var dueSoonReminders = await querySession.Query<DbReminder>()
      .Where(x => !x.IsDeleted && x.ReminderTime >= nowUtc && x.ReminderTime <= remindersWindowEnd)
      .CountAsync(cancellationToken);
    var sentTodayReminders = await querySession.Query<DbReminder>()
      .Where(x => !x.IsDeleted && x.SentOn >= dayStart)
      .CountAsync(cancellationToken);
    var acknowledgedReminders = await querySession.Query<DbReminder>()
      .Where(x => !x.IsDeleted && x.AcknowledgedOn.HasValue)
      .CountAsync(cancellationToken);

    var totalConversations = await querySession.Query<DbConversation>().CountAsync(cancellationToken);
    var messagesLast24Hours = (await querySession.Query<DbConversation>()
      .Where(x => x.UpdatedOn >= messagesWindowStart)
      .Select(x => x.Messages.Count(m => m.CreatedOn >= messagesWindowStart))
      .ToListAsync(cancellationToken))
      .Sum();

    var recentToDos = await querySession.Query<DbToDo>()
      .Where(x => !x.IsDeleted)
      .OrderByDescending(x => x.CreatedOn)
      .Take(6)
      .Select(x => new ToDoActivityProjection(x.PersonId, x.Description, x.CreatedOn))
      .ToListAsync(cancellationToken);

    var recentReminders = await querySession.Query<DbReminder>()
      .Where(x => !x.IsDeleted)
      .OrderByDescending(x => x.SentOn ?? x.CreatedOn)
      .Take(6)
      .Select(x => new ReminderActivityProjection(x.PersonId, x.Details, x.CreatedOn, x.SentOn))
      .ToListAsync(cancellationToken);

    var recentConversations = await querySession.Query<DbConversation>()
      .OrderByDescending(x => x.UpdatedOn)
      .Take(6)
      .Select(x => new ConversationActivityProjection(x.PersonId, x.UpdatedOn, x.Messages))
      .ToListAsync(cancellationToken);

    var activityPersonIds = recentToDos.Select(x => x.PersonId)
      .Concat(recentReminders.Select(x => x.PersonId))
      .Concat(recentConversations.Select(x => x.PersonId))
      .Distinct()
      .ToArray();

    var peopleById = activityPersonIds.Length == 0
      ? new Dictionary<Guid, string>()
      : (await querySession.Query<DbPerson>()
          .Where(x => activityPersonIds.Contains(x.Id))
          .Select(x => new PersonLookupProjection(x.Id, x.Username))
          .ToListAsync(cancellationToken))
          .ToDictionary(x => x.Id, x => x.Username);

    var activity = BuildRecentActivity(recentToDos, recentReminders, recentConversations, peopleById)
      .OrderByDescending(x => x.OccurredOnUtc)
      .Take(8)
      .Select((item, index) => item with { Id = index })
      .ToArray();

    return new DashboardOverviewData
    {
      People = new DashboardPeopleData
      {
        Total = totalPeople,
        WithAccess = peopleWithAccess,
      },
      ToDos = new DashboardToDoData
      {
        Active = activeToDos,
        Completed = completedToDos,
        CreatedToday = createdTodayToDos,
      },
      Reminders = new DashboardReminderData
      {
        Scheduled = scheduledReminders,
        DueWithin24Hours = dueSoonReminders,
        SentToday = sentTodayReminders,
        Acknowledged = acknowledgedReminders,
      },
      Conversations = new DashboardConversationData
      {
        Total = totalConversations,
        MessagesLast24Hours = messagesLast24Hours,
      },
      Activity = activity,
    };
  }

  private static IEnumerable<DashboardActivityData> BuildRecentActivity(
    IEnumerable<ToDoActivityProjection> toDos,
    IEnumerable<ReminderActivityProjection> reminders,
    IEnumerable<ConversationActivityProjection> conversations,
    IReadOnlyDictionary<Guid, string> peopleById)
  {
    foreach (var toDo in toDos)
    {
      yield return new DashboardActivityData
      {
        Id = 0,
        Kind = "todo_created",
        Title = "To-do created",
        Description = $"{ResolveUsername(peopleById, toDo.PersonId)} added {Truncate(toDo.Description, 80)}",
        OccurredOnUtc = toDo.CreatedOn,
      };
    }

    foreach (var reminder in reminders)
    {
      var wasSent = reminder.SentOn.HasValue;

      yield return new DashboardActivityData
      {
        Id = 0,
        Kind = wasSent ? "reminder_sent" : "reminder_scheduled",
        Title = wasSent ? "Reminder sent" : "Reminder scheduled",
        Description = $"{ResolveUsername(peopleById, reminder.PersonId)}: {Truncate(reminder.Details, 80)}",
        OccurredOnUtc = reminder.SentOn ?? reminder.CreatedOn,
      };
    }

    foreach (var conversation in conversations)
    {
      if (conversation.Messages.Count == 0)
      {
        continue;
      }

      var latestMessage = conversation.Messages[^1];

      yield return new DashboardActivityData
      {
        Id = 0,
        Kind = latestMessage.FromUser ? "user_message" : "apollo_message",
        Title = latestMessage.FromUser ? "User message" : "Apollo reply",
        Description = $"{ResolveUsername(peopleById, conversation.PersonId)}: {Truncate(latestMessage.Content, 80)}",
        OccurredOnUtc = latestMessage.CreatedOn,
      };
    }
  }

  private static string ResolveUsername(IReadOnlyDictionary<Guid, string> peopleById, Guid personId)
  {
    return peopleById.TryGetValue(personId, out var username) ? username : "Unknown user";
  }

  private static string Truncate(string value, int maxLength)
  {
    if (value.Length <= maxLength)
    {
      return value;
    }

    if (maxLength <= 3)
    {
      return value[..maxLength];
    }

    return $"{value[..(maxLength - 3)]}...";
  }

  private sealed record PersonLookupProjection(Guid Id, string Username);

  private sealed record ToDoActivityProjection(Guid PersonId, string Description, DateTime CreatedOn);

  private sealed record ReminderActivityProjection(Guid PersonId, string Details, DateTime CreatedOn, DateTime? SentOn);

  private sealed record ConversationActivityProjection(Guid PersonId, DateTime UpdatedOn, IReadOnlyList<DbMessage> Messages);
}
