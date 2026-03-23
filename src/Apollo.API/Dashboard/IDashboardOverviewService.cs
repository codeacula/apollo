namespace Apollo.API.Dashboard;

public interface IDashboardOverviewService
{
  Task<DashboardOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);
  string CreateSignature(DashboardOverviewResponse overview);
}
