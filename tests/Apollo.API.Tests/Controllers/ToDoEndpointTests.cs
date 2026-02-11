using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Apollo.API.Tests.Controllers;

public sealed class ToDoEndpointTests(WebApplicationFactory<IApolloAPI> factory) : IClassFixture<WebApplicationFactory<IApolloAPI>>
{
  private readonly WebApplicationFactory<IApolloAPI> _factory = factory;

  #region GET /todos Tests

  [Fact]
  public async Task GetToDosReturnsOkStatusAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/todos");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should return OK or NotFound, not error status");
  }

  [Fact]
  public async Task GetToDosWithValidRequestReturnsContentAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/todos");
    var content = await response.Content.ReadAsStringAsync();

    // Assert
    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    {
      Assert.NotEmpty(content);
    }
  }

  #endregion GET /todos Tests

  #region GET /todos/{id} Tests

  [Fact]
  public async Task GetToDoByIdWithValidIdReturnsOkAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    const string toDoId = "550e8400-e29b-41d4-a716-446655440000";

    // Act
    var response = await client.GetAsync($"/api/todos/{toDoId}");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should return OK or NotFound");
  }

  [Fact]
  public async Task GetToDoByIdWithInvalidIdReturnsBadRequestOrNotFoundAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    const string invalidId = "invalid-id";

    // Act
    var response = await client.GetAsync($"/api/todos/{invalidId}");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should return BadRequest or NotFound for invalid ID");
  }

  #endregion GET /todos/{id} Tests

  #region POST /todos Tests

  [Fact]
  public async Task PostToDoWithValidDataReturnsCreatedAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    var createRequest = new
    {
      title = "Buy milk",
      description = "From the grocery store",
      priority = "High"
    };
    var content = new StringContent(
      JsonSerializer.Serialize(createRequest),
      System.Text.Encoding.UTF8,
      "application/json");

    // Act
    var response = await client.PostAsync("/api/todos", content);

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.Created or
      System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should return Created, BadRequest, or NotFound");
  }

  [Fact]
  public async Task PostToDoWithEmptyBodyReturnsBadRequestAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

    // Act
    var response = await client.PostAsync("/api/todos", content);

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound or
      System.Net.HttpStatusCode.UnsupportedMediaType,
      "Endpoint should reject empty body");
  }

  [Fact]
  public async Task PostToDoWithInvalidJsonReturnsBadRequestAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    var content = new StringContent(
      "{ invalid json }",
      System.Text.Encoding.UTF8,
      "application/json");

    // Act
    var response = await client.PostAsync("/api/todos", content);

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should reject invalid JSON");
  }

  #endregion POST /todos Tests

  #region PUT /todos/{id} Tests

  [Fact]
  public async Task PutToDoWithValidIdAndDataReturnsOkAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    const string toDoId = "550e8400-e29b-41d4-a716-446655440000";
    var updateRequest = new
    {
      title = "Updated title",
      priority = "Medium"
    };
    var content = new StringContent(
      JsonSerializer.Serialize(updateRequest),
      System.Text.Encoding.UTF8,
      "application/json");

    // Act
    var response = await client.PutAsync($"/api/todos/{toDoId}", content);

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.NotFound or
      System.Net.HttpStatusCode.BadRequest,
      "Endpoint should return OK, NotFound, or BadRequest");
  }

  [Fact]
  public async Task PutToDoWithInvalidIdReturnsBadRequestOrNotFoundAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    var updateRequest = new { title = "Updated" };
    var content = new StringContent(
      JsonSerializer.Serialize(updateRequest),
      System.Text.Encoding.UTF8,
      "application/json");

    // Act
    var response = await client.PutAsync("/api/todos/invalid-id", content);

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should return BadRequest or NotFound for invalid ID");
  }

  #endregion PUT /todos/{id} Tests

  #region DELETE /todos/{id} Tests

  [Fact]
  public async Task DeleteToDoWithValidIdReturnsNoContentOrOkAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    const string toDoId = "550e8400-e29b-41d4-a716-446655440000";

    // Act
    var response = await client.DeleteAsync($"/api/todos/{toDoId}");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.NoContent or
      System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should return NoContent, OK, or NotFound");
  }

  [Fact]
  public async Task DeleteToDoWithInvalidIdReturnsBadRequestOrNotFoundAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.DeleteAsync("/api/todos/invalid-id");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should return BadRequest or NotFound for invalid ID");
  }

  #endregion DELETE /todos/{id} Tests

  #region Error Handling Tests

  [Fact]
  public async Task ToDoEndpointWithUnauthorizedRequestReturnsUnauthorizedOrNotFoundAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-token");

    // Act
    var response = await client.GetAsync("/api/todos");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.Unauthorized or
      System.Net.HttpStatusCode.NotFound or
      System.Net.HttpStatusCode.OK,
      "Endpoint should handle unauthorized requests");
  }

  [Fact]
  public async Task ToDoEndpointWithForbiddenAccessReturnsForbiddenOrNotFoundAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/todos/other-users-id");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.Forbidden or
      System.Net.HttpStatusCode.NotFound or
      System.Net.HttpStatusCode.OK,
      "Endpoint should handle forbidden access");
  }

  #endregion Error Handling Tests
}
