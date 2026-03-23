using System.Text.Json;

using Apollo.Application.Configuration;
using Apollo.Core.Dashboard;

using MediatR;

namespace Apollo.API.Dashboard;

public sealed class DashboardOverviewService(
  IMediator mediator,
  IDashboardOverviewStore dashboardOverviewStore) : IDashboardOverviewService
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
    var overviewData = await dashboardOverviewStore.GetOverviewDataAsync(now, cancellationToken);

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
        Total = overviewData.People.Total,
        WithAccess = overviewData.People.WithAccess,
      },
      ToDos = new DashboardToDoSummaryResponse
      {
        Active = overviewData.ToDos.Active,
        Completed = overviewData.ToDos.Completed,
        CreatedToday = overviewData.ToDos.CreatedToday,
      },
      Reminders = new DashboardReminderSummaryResponse
      {
        Scheduled = overviewData.Reminders.Scheduled,
        DueWithin24Hours = overviewData.Reminders.DueWithin24Hours,
        SentToday = overviewData.Reminders.SentToday,
        Acknowledged = overviewData.Reminders.Acknowledged,
      },
      Conversations = new DashboardConversationSummaryResponse
      {
        Total = overviewData.Conversations.Total,
        MessagesLast24Hours = overviewData.Conversations.MessagesLast24Hours,
      },
      Activity = overviewData.Activity.Select(x => new DashboardActivityItemResponse
      {
        Kind = x.Kind,
        Title = x.Title,
        Description = x.Description,
        OccurredOnUtc = x.OccurredOnUtc,
      }).ToArray(),
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
}
