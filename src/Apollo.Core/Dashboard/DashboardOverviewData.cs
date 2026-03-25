namespace Apollo.Core.Dashboard;

public sealed record DashboardOverviewData
{
  public required DashboardPeopleData People { get; init; }
  public required DashboardToDoData ToDos { get; init; }
  public required DashboardReminderData Reminders { get; init; }
  public required DashboardConversationData Conversations { get; init; }
  public required DashboardActivityData[] Activity { get; init; }
}

public sealed record DashboardPeopleData
{
  public required int Total { get; init; }
  public required int WithAccess { get; init; }
}

public sealed record DashboardToDoData
{
  public required int Active { get; init; }
  public required int Completed { get; init; }
  public required int CreatedToday { get; init; }
}

public sealed record DashboardReminderData
{
  public required int Scheduled { get; init; }
  public required int DueWithin24Hours { get; init; }
  public required int SentToday { get; init; }
  public required int Acknowledged { get; init; }
}

public sealed record DashboardConversationData
{
  public required int Total { get; init; }
  public required int MessagesLast24Hours { get; init; }
}

public sealed record DashboardActivityData
{
  public required int Id { get; init; }
  public required string Kind { get; init; }
  public required string Title { get; init; }
  public required string Description { get; init; }
  public required DateTime OccurredOnUtc { get; init; }
}
