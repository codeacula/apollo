namespace Apollo.API.Dashboard;

public sealed record DashboardOverviewResponse
{
  public required DateTime GeneratedAtUtc { get; init; }
  public required DashboardConfigurationStatusResponse Configuration { get; init; }
  public required DashboardPeopleSummaryResponse People { get; init; }
  public required DashboardToDoSummaryResponse ToDos { get; init; }
  public required DashboardReminderSummaryResponse Reminders { get; init; }
  public required DashboardConversationSummaryResponse Conversations { get; init; }
  public required DashboardActivityItemResponse[] Activity { get; init; }
}

public sealed record DashboardConfigurationStatusResponse
{
  public required bool IsInitialized { get; init; }
  public required bool IsConfigured { get; init; }
  public required DashboardSubsystemStatusResponse Subsystems { get; init; }
}

public sealed record DashboardSubsystemStatusResponse
{
  public required bool Ai { get; init; }
  public required bool Discord { get; init; }
  public required bool SuperAdmin { get; init; }
}

public sealed record DashboardPeopleSummaryResponse
{
  public required int Total { get; init; }
  public required int WithAccess { get; init; }
}

public sealed record DashboardToDoSummaryResponse
{
  public required int Active { get; init; }
  public required int Completed { get; init; }
  public required int CreatedToday { get; init; }
}

public sealed record DashboardReminderSummaryResponse
{
  public required int Scheduled { get; init; }
  public required int DueWithin24Hours { get; init; }
  public required int SentToday { get; init; }
  public required int Acknowledged { get; init; }
}

public sealed record DashboardConversationSummaryResponse
{
  public required int Total { get; init; }
  public required int MessagesLast24Hours { get; init; }
}

public sealed record DashboardActivityItemResponse
{
  public required string Kind { get; init; }
  public required string Title { get; init; }
  public required string Description { get; init; }
  public required DateTime OccurredOnUtc { get; init; }
}
