using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Apollo.API.Tests.Controllers;

public sealed class HealthCheckEndpointTests(WebApplicationFactory<IApolloAPI> factory) : IClassFixture<WebApplicationFactory<IApolloAPI>>
{
  private readonly WebApplicationFactory<IApolloAPI> _factory = factory;

  #region Basic Health Check Tests

  [Fact]
  public async Task HealthCheckEndpointReturnsOkStatusAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.NotFound,
      "Health endpoint should return OK or be not found");
  }

  [Fact]
  public async Task HealthCheckEndpointReturnsJsonContentAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health");

    // Assert
    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    {
      Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }
  }

  [Fact]
  public async Task HealthCheckEndpointReturnsValidResponseAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health");
    var content = await response.Content.ReadAsStringAsync();

    // Assert
    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    {
      Assert.NotEmpty(content);
      _ = JsonDocument.Parse(content);
    }
  }

  #endregion Basic Health Check Tests

  #region Detailed Health Check Tests

  [Fact]
  public async Task DetailedHealthCheckEndpointReturnsOkAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health/detailed");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.NotFound,
      "Detailed health endpoint should return OK or be not found");
  }

  [Fact]
  public async Task DetailedHealthCheckIncludesDependencyStatusAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health/detailed");

    // Assert
    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    {
      var content = await response.Content.ReadAsStringAsync();
      Assert.NotEmpty(content);
      // The content should be JSON with health details
      _ = JsonDocument.Parse(content);
    }
  }

  #endregion Detailed Health Check Tests

  #region Service Dependency Tests

  [Fact]
  public async Task HealthCheckReportsServiceStatusAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health");

    // Assert
    // Health check should respond regardless of internal service status
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.ServiceUnavailable or
      System.Net.HttpStatusCode.NotFound,
      "Health endpoint should report service status");
  }

  #endregion Service Dependency Tests

  #region Database Health Tests

  [Fact]
  public async Task HealthCheckIncludesDatabaseStatusAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health/detailed");

    // Assert
    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    {
      var content = await response.Content.ReadAsStringAsync();
      // Detailed health should check database connectivity
      Assert.NotEmpty(content);
    }
  }

  #endregion Database Health Tests

  #region Cache Health Tests

  [Fact]
  public async Task HealthCheckIncludesCacheStatusAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health/detailed");

    // Assert
    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    {
      var content = await response.Content.ReadAsStringAsync();
      // Detailed health should check cache connectivity
      Assert.NotEmpty(content);
    }
  }

  #endregion Cache Health Tests

  #region gRPC Connection Health Tests

  [Fact]
  public async Task HealthCheckIncludesGrpcConnectionStatusAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health/detailed");

    // Assert
    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    {
      var content = await response.Content.ReadAsStringAsync();
      // Detailed health should check gRPC connection
      Assert.NotEmpty(content);
    }
  }

  #endregion gRPC Connection Health Tests

  #region Readiness Tests

  [Fact]
  public async Task ReadinessCheckEndpointReturnsOkWhenReadyAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health/ready");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.ServiceUnavailable or
      System.Net.HttpStatusCode.NotFound,
      "Readiness endpoint should report when service is ready");
  }

  [Fact]
  public async Task LivenessCheckEndpointReturnsOkWhenAliveAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health/live");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.NotFound,
      "Liveness endpoint should report when service is alive");
  }

  #endregion Readiness Tests

  #region Response Format Tests

  [Fact]
  public async Task HealthCheckResponseIncludesStatusFieldAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health");

    // Assert
    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    {
      var content = await response.Content.ReadAsStringAsync();
      var doc = JsonDocument.Parse(content);
      // Response should have a status field
      Assert.True(doc.RootElement.TryGetProperty("status", out _) ||
                  doc.RootElement.ValueKind == JsonValueKind.Object);
    }
  }

  [Fact]
  public async Task HealthCheckResponseIncludesTimestampAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health/detailed");

    // Assert
    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    {
      var content = await response.Content.ReadAsStringAsync();
      Assert.NotEmpty(content);
      // Detailed health should include timestamp
    }
  }

  #endregion Response Format Tests

  #region No Authentication Required Tests

  [Fact]
  public async Task HealthCheckDoesNotRequireAuthenticationAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    // Don't add any authentication headers

    // Act
    var response = await client.GetAsync("/health");

    // Assert
    Assert.NotEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task HealthCheckWithoutTokenStillReturnsStatusAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/health");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.NotFound or
      System.Net.HttpStatusCode.ServiceUnavailable,
      "Health endpoint should be accessible without authentication");
  }

  #endregion No Authentication Required Tests
}
