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

    var people = await querySession.Query<DbPerson>().ToListAsync(cancellationToken);
    var toDos = await querySession.Query<DbToDo>()
      .Where(x => !x.IsDeleted)
      .ToListAsync(cancellationToken);
    var reminders = await querySession.Query<DbReminder>()
      .Where(x => !x.IsDeleted)
      .ToListAsync(cancellationToken);
    var conversations = await querySession.Query<DbConversation>().ToListAsync(cancellationToken);

    var now = TimeProvider.System.GetUtcNow().UtcDateTime;
    var status = statusResult.Value;
    var peopleById = people.ToDictionary(x => x.Id, x => x.Username);

    var activity = BuildRecentActivity(toDos, reminders, conversations, peopleById)
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
        Total = people.Count,
        WithAccess = people.Count(x => x.HasAccess),
      },
      ToDos = new DashboardToDoSummaryResponse
      {
        Active = toDos.Count(x => !x.IsCompleted),
        Completed = toDos.Count(x => x.IsCompleted),
        CreatedToday = toDos.Count(x => x.CreatedOn >= now.Date),
      },
      Reminders = new DashboardReminderSummaryResponse
      {
        Scheduled = reminders.Count,
        DueWithin24Hours = reminders.Count(x => x.ReminderTime >= now && x.ReminderTime <= now.AddHours(24)),
        SentToday = reminders.Count(x => x.SentOn >= now.Date),
        Acknowledged = reminders.Count(x => x.AcknowledgedOn.HasValue),
      },
      Conversations = new DashboardConversationSummaryResponse
      {
        Total = conversations.Count,
        MessagesLast24Hours = conversations.Sum(x => x.Messages.Count(m => m.CreatedOn >= now.AddHours(-24))),
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
    IEnumerable<DbToDo> toDos,
    IEnumerable<DbReminder> reminders,
    IEnumerable<DbConversation> conversations,
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
      var latestMessage = conversation.Messages.LastOrDefault();

      if (latestMessage is null)
      {
        continue;
      }

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
