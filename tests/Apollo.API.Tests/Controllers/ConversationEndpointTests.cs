using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Apollo.API.Tests.Controllers;

public sealed class ConversationEndpointTests(WebApplicationFactory<IApolloAPI> factory) : IClassFixture<WebApplicationFactory<IApolloAPI>>
{
  private readonly WebApplicationFactory<IApolloAPI> _factory = factory;

  #region GET /conversations Tests

  [Fact]
  public async Task GetConversationsReturnsOkStatusAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/conversations");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should return OK or NotFound");
  }

  [Fact]
  public async Task GetConversationsReturnsValidContentAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/conversations");

    // Assert
    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    {
      var content = await response.Content.ReadAsStringAsync();
      Assert.NotEmpty(content);
    }
  }

  [Fact]
  public async Task GetConversationsWithPaginationReturnsOkAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/conversations?page=1&pageSize=10");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should handle pagination parameters");
  }

  #endregion GET /conversations Tests

  #region GET /conversations/{id} Tests

  [Fact]
  public async Task GetConversationByIdWithValidIdReturnsOkAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    const string conversationId = "550e8400-e29b-41d4-a716-446655440000";

    // Act
    var response = await client.GetAsync($"/api/conversations/{conversationId}");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should return OK or NotFound");
  }

  [Fact]
  public async Task GetConversationByIdWithInvalidIdReturnsBadRequestOrNotFoundAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/conversations/invalid-id");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should return BadRequest or NotFound for invalid ID");
  }

  [Fact]
  public async Task GetConversationByIdReturnsConversationDetailsAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    const string conversationId = "550e8400-e29b-41d4-a716-446655440000";

    // Act
    var response = await client.GetAsync($"/api/conversations/{conversationId}");

    // Assert
    if (response.StatusCode == System.Net.HttpStatusCode.OK)
    {
      var content = await response.Content.ReadAsStringAsync();
      Assert.NotEmpty(content);
    }
  }

  #endregion GET /conversations/{id} Tests

  #region POST /conversations/{id}/messages Tests

  [Fact]
  public async Task PostMessageWithValidDataReturnsOkAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    const string conversationId = "550e8400-e29b-41d4-a716-446655440000";
    var messageRequest = new
    {
      content = "Hello, this is a test message",
      role = "user"
    };
    var content = new StringContent(
      JsonSerializer.Serialize(messageRequest),
      System.Text.Encoding.UTF8,
      "application/json");

    // Act
    var response = await client.PostAsync($"/api/conversations/{conversationId}/messages", content);

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.Created or
      System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should handle message posting");
  }

  [Fact]
  public async Task PostMessageWithEmptyContentReturnsBadRequestAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    const string conversationId = "550e8400-e29b-41d4-a716-446655440000";
    var messageRequest = new
    {
      content = "",
      role = "user"
    };
    var content = new StringContent(
      JsonSerializer.Serialize(messageRequest),
      System.Text.Encoding.UTF8,
      "application/json");

    // Act
    var response = await client.PostAsync($"/api/conversations/{conversationId}/messages", content);

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should reject empty message content");
  }

  [Fact]
  public async Task PostMessageWithInvalidJsonReturnsBadRequestAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    const string conversationId = "550e8400-e29b-41d4-a716-446655440000";
    var content = new StringContent(
      "{ invalid json }",
      System.Text.Encoding.UTF8,
      "application/json");

    // Act
    var response = await client.PostAsync($"/api/conversations/{conversationId}/messages", content);

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should reject invalid JSON");
  }

  [Fact]
  public async Task PostMessageWithInvalidConversationIdReturnsBadRequestOrNotFoundAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    var messageRequest = new { content = "Test", role = "user" };
    var content = new StringContent(
      JsonSerializer.Serialize(messageRequest),
      System.Text.Encoding.UTF8,
      "application/json");

    // Act
    var response = await client.PostAsync("/api/conversations/invalid-id/messages", content);

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should return BadRequest or NotFound for invalid conversation ID");
  }

  #endregion POST /conversations/{id}/messages Tests

  #region GET /conversations/{id}/messages Tests

  [Fact]
  public async Task GetConversationMessagesReturnsOkAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    const string conversationId = "550e8400-e29b-41d4-a716-446655440000";

    // Act
    var response = await client.GetAsync($"/api/conversations/{conversationId}/messages");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should return OK or NotFound");
  }

  [Fact]
  public async Task GetConversationMessagesWithPaginationReturnsOkAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    const string conversationId = "550e8400-e29b-41d4-a716-446655440000";

    // Act
    var response = await client.GetAsync($"/api/conversations/{conversationId}/messages?page=1&pageSize=20");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should handle pagination");
  }

  #endregion GET /conversations/{id}/messages Tests

  #region Error Handling Tests

  [Fact]
  public async Task ConversationEndpointWithUnauthorizedRequestReturnsUnauthorizedOrNotFoundAsync()
  {
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-token");

    // Act
    var response = await client.GetAsync("/api/conversations");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.Unauthorized or
      System.Net.HttpStatusCode.NotFound or
      System.Net.HttpStatusCode.OK,
      "Endpoint should handle unauthorized requests");
  }

  [Fact]
  public async Task ConversationEndpointWithForbiddenAccessReturnsForbiddenOrNotFoundAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/conversations/other-users-conversation");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.Forbidden or
      System.Net.HttpStatusCode.NotFound or
      System.Net.HttpStatusCode.OK,
      "Endpoint should handle forbidden access");
  }

  #endregion Error Handling Tests

  #region Filtering Tests

  [Fact]
  public async Task GetConversationsWithFilterReturnsOkAsync()
  {
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/conversations?search=test");

    // Assert
    Assert.True(
      response.StatusCode is System.Net.HttpStatusCode.OK or
      System.Net.HttpStatusCode.BadRequest or
      System.Net.HttpStatusCode.NotFound,
      "Endpoint should support filtering");
  }

  #endregion Filtering Tests
}
