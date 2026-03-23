using System.Text.Json;

using Apollo.Application.Configuration;
using Apollo.Database.Conversations;
using Apollo.Database.People;
using Apollo.Database.ToDos;

using Marten;

using MediatR;

namespace Apollo.API.Dashboard;

public sealed class DashboardOverviewService(
  IMediator mediator,
  IQuerySession querySession) : IDashboardOverviewService
{
  public async Task<DashboardOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
  {
    var statusResult = await mediator.Send(new GetInitializationStatusQuery(), cancellationToken);

    if (statusResult.IsFailed)
    {
      throw new InvalidOperationException(statusResult.Errors.Count > 0 ? statusResult.Errors[0].Message : "Unknown error");
    }

    var now = TimeProvider.System.GetUtcNow().UtcDateTime;
    var status = statusResult.Value;
    var dayStart = now.Date;
    var messagesWindowStart = now.AddHours(-24);
    var remindersWindowEnd = now.AddHours(24);

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
      .Where(x => !x.IsDeleted && x.ReminderTime >= now && x.ReminderTime <= remindersWindowEnd)
      .CountAsync(cancellationToken);
    var sentTodayReminders = await querySession.Query<DbReminder>()
      .Where(x => !x.IsDeleted && x.SentOn >= dayStart)
      .CountAsync(cancellationToken);
    var acknowledgedReminders = await querySession.Query<DbReminder>()
      .Where(x => !x.IsDeleted && x.AcknowledgedOn.HasValue)
      .CountAsync(cancellationToken);

    var totalConversations = await querySession.Query<DbConversation>().CountAsync(cancellationToken);
    var recentConversationMessages = await querySession.Query<DbConversation>()
      .Where(x => x.UpdatedOn >= messagesWindowStart)
      .Select(x => new ConversationMessagesProjection(x.Messages))
      .ToListAsync(cancellationToken);
    var messagesLast24Hours = recentConversationMessages.Sum(x => x.Messages.Count(m => m.CreatedOn >= messagesWindowStart));

    var peopleById = (await querySession.Query<DbPerson>()
      .Select(x => new PersonLookupProjection(x.Id, x.Username))
      .ToListAsync(cancellationToken))
      .ToDictionary(x => x.Id, x => x.Username);

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

    var activity = BuildRecentActivity(recentToDos, recentReminders, recentConversations, peopleById)
      .OrderByDescending(x => x.OccurredOnUtc)
      .Take(8)
      .ToArray();

    return new DashboardOverviewResponse
    {
      GeneratedAtUtc = now,
      Configuration = new DashboardConfigurationStatusResponse
      {
        IsInitialized = status.IsInitialized,
        IsConfigured = status.IsAiConfigured && status.IsDiscordConfigured && status.IsSuperAdminConfigured,
        Subsystems = new DashboardSubsystemStatusResponse
        {
          Ai = status.IsAiConfigured,
          Discord = status.IsDiscordConfigured,
          SuperAdmin = status.IsSuperAdminConfigured,
        }
      },
      People = new DashboardPeopleSummaryResponse
      {
        Total = totalPeople,
        WithAccess = peopleWithAccess,
      },
      ToDos = new DashboardToDoSummaryResponse
      {
        Active = activeToDos,
        Completed = completedToDos,
        CreatedToday = createdTodayToDos,
      },
      Reminders = new DashboardReminderSummaryResponse
      {
        Scheduled = scheduledReminders,
        DueWithin24Hours = dueSoonReminders,
        SentToday = sentTodayReminders,
        Acknowledged = acknowledgedReminders,
      },
      Conversations = new DashboardConversationSummaryResponse
      {
        Total = totalConversations,
        MessagesLast24Hours = messagesLast24Hours,
      },
      Activity = activity,
    };
  }

  public string CreateSignature(DashboardOverviewResponse overview)
  {
    return JsonSerializer.Serialize(new
    {
      overview.Configuration,
      overview.People,
      overview.ToDos,
      overview.Reminders,
      overview.Conversations,
      overview.Activity,
    });
  }

  private static IEnumerable<DashboardActivityItemResponse> BuildRecentActivity(
    IEnumerable<ToDoActivityProjection> toDos,
    IEnumerable<ReminderActivityProjection> reminders,
    IEnumerable<ConversationActivityProjection> conversations,
    IReadOnlyDictionary<Guid, string> peopleById)
  {
    foreach (var toDo in toDos.OrderByDescending(x => x.CreatedOn).Take(6))
    {
      yield return new DashboardActivityItemResponse
      {
        Kind = "todo_created",
        Title = "To-do created",
        Description = $"{ResolveUsername(peopleById, toDo.PersonId)} added {Truncate(toDo.Description, 80)}",
        OccurredOnUtc = toDo.CreatedOn,
      };
    }

    foreach (var reminder in reminders.OrderByDescending(x => x.SentOn ?? x.CreatedOn).Take(6))
    {
      var wasSent = reminder.SentOn.HasValue;

      yield return new DashboardActivityItemResponse
      {
        Kind = wasSent ? "reminder_sent" : "reminder_scheduled",
        Title = wasSent ? "Reminder sent" : "Reminder scheduled",
        Description = $"{ResolveUsername(peopleById, reminder.PersonId)}: {Truncate(reminder.Details, 80)}",
        OccurredOnUtc = reminder.SentOn ?? reminder.CreatedOn,
      };
    }

    foreach (var conversation in conversations.OrderByDescending(x => x.UpdatedOn).Take(6))
    {
      if (conversation.Messages.Count == 0)
      {
        continue;
      }

      var latestMessage = conversation.Messages[^1];

      yield return new DashboardActivityItemResponse
      {
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

  private sealed record PersonLookupProjection(Guid Id, string Username);

  private sealed record ToDoActivityProjection(Guid PersonId, string Description, DateTime CreatedOn);

  private sealed record ReminderActivityProjection(Guid PersonId, string Details, DateTime CreatedOn, DateTime? SentOn);

  private sealed record ConversationActivityProjection(Guid PersonId, DateTime UpdatedOn, IReadOnlyList<DbMessage> Messages);

  private sealed record ConversationMessagesProjection(IReadOnlyList<DbMessage> Messages);

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
}
