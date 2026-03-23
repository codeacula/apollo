namespace Apollo.Core.Dashboard;

public interface IDashboardUpdatePublisher
{
  Task PublishOverviewUpdatedAsync(CancellationToken cancellationToken = default);
}
