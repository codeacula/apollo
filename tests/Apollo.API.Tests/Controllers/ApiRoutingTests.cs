using Microsoft.AspNetCore.Mvc.Testing;

namespace Apollo.API.Tests.Controllers;

public class ApiRoutingTests(WebApplicationFactory<IApolloAPI> factory) : IClassFixture<WebApplicationFactory<IApolloAPI>>
{
  private readonly WebApplicationFactory<IApolloAPI> _factory = factory;

  [Fact]
  public async Task GetRootReturnsValidResponseBasedOnSpaAssetAvailabilityAsync()
  {
    var client = _factory.CreateClient();

    var response = await client.GetAsync("/");

    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.NotFound,
      $"Unexpected status code: {response.StatusCode}");
  }

  [Fact]
  public async Task GetDashboardRouteReturnsValidSpaResponseBasedOnAssetAvailabilityAsync()
  {
    var client = _factory.CreateClient();

    var response = await client.GetAsync("/dashboard");

    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.NotFound,
      $"Unexpected status code: {response.StatusCode}");
  }

  [Fact]
  public async Task GetNonExistentEndpointReturnsNotFoundAsync()
  {
    var client = _factory.CreateClient();

    var response = await client.GetAsync("/api/nonexistent");

    Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public void FactoryCreatesClientSuccessfully()
  {
    var client = _factory.CreateClient();

    Assert.NotNull(client);
    Assert.NotNull(client.BaseAddress);
  }
}
