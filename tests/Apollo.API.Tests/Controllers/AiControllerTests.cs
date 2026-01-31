using Microsoft.AspNetCore.Mvc.Testing;

namespace Apollo.API.Tests.Controllers;

public class AiControllerTests : IClassFixture<WebApplicationFactory<IApolloAPI>>
{
  private readonly WebApplicationFactory<IApolloAPI> _factory;

  public AiControllerTests(WebApplicationFactory<IApolloAPI> factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task GetRootReturnsNotFoundAsync()
  {
    var client = _factory.CreateClient();

    var response = await client.GetAsync("/");

    Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
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
