namespace Apollo.Core.Dashboard;

public interface IDashboardOverviewStore
{
  Task<DashboardOverviewData> GetOverviewDataAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
}
