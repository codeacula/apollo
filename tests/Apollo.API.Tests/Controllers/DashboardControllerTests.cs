using Apollo.API.Dashboard;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace Apollo.API.Tests.Controllers;

public sealed class DashboardControllerTests(WebApplicationFactory<IApolloAPI> factory) : IClassFixture<WebApplicationFactory<IApolloAPI>>
{
  private readonly WebApplicationFactory<IApolloAPI> _factory = factory;

  [Fact]
  public async Task GetOverviewReturnsDashboardPayloadAsync()
  {
    var mockService = new Mock<IDashboardOverviewService>();
    mockService
      .Setup(x => x.GetOverviewAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(new DashboardOverviewResponse
      {
        GeneratedAtUtc = new DateTime(2026, 3, 22, 20, 0, 0, DateTimeKind.Utc),
        Configuration = new DashboardConfigurationStatusResponse
        {
          IsInitialized = true,
          IsConfigured = true,
          Subsystems = new DashboardSubsystemStatusResponse
          {
            Ai = true,
            Discord = true,
            SuperAdmin = true,
          }
        },
        People = new DashboardPeopleSummaryResponse
        {
          Total = 2,
          WithAccess = 1,
        },
        ToDos = new DashboardToDoSummaryResponse
        {
          Active = 3,
          Completed = 4,
          CreatedToday = 1,
        },
        Reminders = new DashboardReminderSummaryResponse
        {
          Scheduled = 5,
          DueWithin24Hours = 2,
          SentToday = 1,
          Acknowledged = 1,
        },
        Conversations = new DashboardConversationSummaryResponse
        {
          Total = 2,
          MessagesLast24Hours = 8,
        },
        Activity =
        [
          new DashboardActivityItemResponse
          {
            Id = 0,
            Kind = "todo_created",
            Title = "To-do created",
            Description = "codeacula added Stretch",
            OccurredOnUtc = new DateTime(2026, 3, 22, 19, 0, 0, DateTimeKind.Utc),
          }
        ]
      });

    var client = CreateClient(mockService);
    var response = await client.GetAsync("/api/dashboard/overview");

    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("\"generatedAtUtc\":\"2026-03-22T20:00:00Z\"", content);
    Assert.Contains("\"active\":3", content);
    Assert.Contains("\"kind\":\"todo_created\"", content);
  }

  [Fact]
  public async Task GetOverviewReturnsBadRequestWhenOverviewFailsAsync()
  {
    var mockService = new Mock<IDashboardOverviewService>();
    mockService
      .Setup(x => x.GetOverviewAsync(It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("dashboard unavailable"));

    var client = CreateClient(mockService);
    var response = await client.GetAsync("/api/dashboard/overview");

    Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("dashboard unavailable", content);
  }

  [Fact]
  public async Task GetOverviewReturnsInternalServerErrorOnUnexpectedExceptionAsync()
  {
    var mockService = new Mock<IDashboardOverviewService>();
    mockService
      .Setup(x => x.GetOverviewAsync(It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidDataException("unexpected failure"));

    var client = CreateClient(mockService);
    var response = await client.GetAsync("/api/dashboard/overview");

    Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
  }

  private HttpClient CreateClient(Mock<IDashboardOverviewService> dashboardOverviewService)
  {
    var factory = _factory.WithWebHostBuilder(builder =>
      _ = builder.ConfigureServices(services => _ = services.AddScoped(_ => dashboardOverviewService.Object)));

    return factory.CreateClient();
  }
}
