using Apollo.API.Controllers;
using Apollo.API.Services;
using Apollo.Core.Configuration;
using Apollo.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apollo.API.Tests.Controllers;

public class ApiControllerTests
{
    private sealed class FakeDiscordMessageSender : IDiscordMessageSender
    {
        private readonly Func<string, CancellationToken, Task<(bool Success, string? Error, ulong? MessageId)>> _sendHandler;
        private readonly Func<ulong, string, string, IEnumerable<ulong>?, CancellationToken, Task<(bool Success, string? Error, ulong? ThreadId, ulong? MessageId)>> _forumHandler;

        public FakeDiscordMessageSender(
            Func<string, CancellationToken, Task<(bool Success, string? Error, ulong? MessageId)>> sendHandler,
            Func<ulong, string, string, IEnumerable<ulong>?, CancellationToken, Task<(bool Success, string? Error, ulong? ThreadId, ulong? MessageId)>>? forumHandler = null)
        {
            _sendHandler = sendHandler;
            _forumHandler = forumHandler ?? ((channelId, title, content, tags, ct) => Task.FromResult((false, "Not implemented", (ulong?)null, (ulong?)null)));
        }

        public Task<(bool Success, string? Error, ulong? MessageId)> SendToDailyAlertAsync(string content, CancellationToken ct)
            => _sendHandler(content, ct);

        public Task<(bool Success, string? Error, ulong? ThreadId, ulong? MessageId)> CreateForumPostAsync(ulong forumChannelId, string title, string content, IEnumerable<ulong>? appliedTagIds, CancellationToken ct)
            => _forumHandler(forumChannelId, title, content, appliedTagIds, ct);
    }

    private sealed class FakeSettingsProvider : ISettingsProvider
    {
        private ApolloSettings _settings;

        public FakeSettingsProvider(ApolloSettings settings)
        {
            _settings = settings;
        }

        public ApolloSettings GetSettings() => _settings;

        public Task ReloadAsync()
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void Ping_ReturnsCorrectResponse()
    {
        var controller = new ApiController(new FakeDiscordMessageSender((c, ct) => Task.FromResult((true, (string?)null, (ulong?)null))), NullLogger<ApiController>.Instance, new FakeSettingsProvider(new ApolloSettings()));

        var result = controller.Ping();

        Assert.Equal("pong", result);
    }

    [Fact]
    public void Ping_ReturnsNonEmptyString()
    {
        var controller = new ApiController(new FakeDiscordMessageSender((c, ct) => Task.FromResult((true, (string?)null, (ulong?)null))), NullLogger<ApiController>.Instance, new FakeSettingsProvider(new ApolloSettings()));

        var result = controller.Ping();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Ping_ReturnsConsistentValue()
    {
        var controller = new ApiController(new FakeDiscordMessageSender((c, ct) => Task.FromResult((true, (string?)null, (ulong?)null))), NullLogger<ApiController>.Instance, new FakeSettingsProvider(new ApolloSettings()));

        var result1 = controller.Ping();
        var result2 = controller.Ping();

        Assert.Equal(result1, result2);
    }

    [Fact]
    public async Task TestMessage_ReturnsOk_OnSuccess()
    {
        // Arrange
        var sender = new FakeDiscordMessageSender((content, ct) => Task.FromResult((true, (string?)null, (ulong?)123456789012345678)));
        var controller = new ApiController(sender, NullLogger<ApiController>.Instance, new FakeSettingsProvider(new ApolloSettings { DailyAlertChannelId = 111111111111111111UL }));

        // Act
        var result = await controller.SendDiscordTestMessage("Hello", CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value!;
        var channelIdProp = value.GetType().GetProperty("channelId");
        var contentProp = value.GetType().GetProperty("content");
        var messageIdProp = value.GetType().GetProperty("messageId");
        Assert.NotNull(contentProp);
        Assert.NotNull(channelIdProp);
        Assert.NotNull(messageIdProp);
        Assert.Equal(111111111111111111UL, channelIdProp!.GetValue(value));
        Assert.Equal("Hello", contentProp!.GetValue(value));
        Assert.Equal(123456789012345678UL, messageIdProp!.GetValue(value));
    }

    [Fact]
    public async Task TestMessage_ReturnsBadRequest_WhenChannelNotConfigured()
    {
        // Arrange
        var sender = new FakeDiscordMessageSender((content, ct) => Task.FromResult((false, "DailyAlertChannelId is not configured.", (ulong?)null)));
        var controller = new ApiController(sender, NullLogger<ApiController>.Instance, new FakeSettingsProvider(new ApolloSettings { DailyAlertChannelId = null }));

        // Act
        var result = await controller.SendDiscordTestMessage("Hello", CancellationToken.None);

        // Assert
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var value = bad.Value!;
        var errorProp = value.GetType().GetProperty("error");
        Assert.NotNull(errorProp);
        Assert.Equal("DailyAlertChannelId is not configured.", errorProp!.GetValue(value));
    }

    [Fact]
    public async Task TestMessage_ReturnsBadGateway_OnSenderFailure()
    {
        // Arrange
        var sender = new FakeDiscordMessageSender((content, ct) => Task.FromResult((false, "Discord REST call failed.", (ulong?)null)));
        var controller = new ApiController(sender, NullLogger<ApiController>.Instance, new FakeSettingsProvider(new ApolloSettings { DailyAlertChannelId = 111111111111111111UL }));

        // Act
        var result = await controller.SendDiscordTestMessage("Hello", CancellationToken.None);

        // Assert
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, obj.StatusCode);
        var value = obj.Value!;
        var errorProp = value.GetType().GetProperty("error");
        Assert.NotNull(errorProp);
        Assert.Equal("Discord REST call failed", errorProp!.GetValue(value));
    }

    [Fact]
    public async Task TestMessage_UsesDefaultContent_WhenMissing()
    {
        // Arrange
        string? capturedContent = null;
        var sender = new FakeDiscordMessageSender((content, ct) =>
        {
            capturedContent = content;
            return Task.FromResult((true, (string?)null, (ulong?)1));
        });
        var controller = new ApiController(sender, NullLogger<ApiController>.Instance, new FakeSettingsProvider(new ApolloSettings { DailyAlertChannelId = 111111111111111111UL }));

        // Act
        var result = await controller.SendDiscordTestMessage(null, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(capturedContent);
        Assert.Contains("Apollo test message from API", capturedContent);
    }

    [Fact]
    public async Task ForumPost_ReturnsBadRequest_WhenChannelIdMissing()
    {
        // Arrange
        var sender = new FakeDiscordMessageSender((c, ct) => Task.FromResult((true, (string?)null, (ulong?)null)));
        var controller = new ApiController(sender, NullLogger<ApiController>.Instance, new FakeSettingsProvider(new ApolloSettings()));

        // Act
        var result = await controller.CreateForumPost(null, "Title", "Body", null, CancellationToken.None);

        // Assert
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var value = bad.Value!;
        var errorProp = value.GetType().GetProperty("error");
        Assert.NotNull(errorProp);
        Assert.Equal("Forum channelId is required.", errorProp!.GetValue(value));
    }

    [Fact]
    public async Task ForumPost_ReturnsOk_OnSuccess()
    {
        // Arrange
        ulong? capturedChannel = null;
        string? capturedTitle = null;
        string? capturedContent = null;
        IEnumerable<ulong>? capturedTags = null;

        var sender = new FakeDiscordMessageSender(
            (c, ct) => Task.FromResult((true, (string?)null, (ulong?)null)),
            (channelId, title, content, tags, ct) =>
            {
                capturedChannel = channelId;
                capturedTitle = title;
                capturedContent = content;
                capturedTags = tags;
                return Task.FromResult((true, (string?)null, (ulong?)222222222222222222UL, (ulong?)333333333333333333UL));
            });

        var controller = new ApiController(sender, NullLogger<ApiController>.Instance, new FakeSettingsProvider(new ApolloSettings()));

        // Act
        var result = await controller.CreateForumPost(111111111111111111UL, "Hello", "World", new[] { 1UL, 2UL }, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(111111111111111111UL, capturedChannel);
        Assert.Equal("Hello", capturedTitle);
        Assert.Equal("World", capturedContent);
        Assert.NotNull(capturedTags);
        var value = ok.Value!;
        var channelIdProp = value.GetType().GetProperty("channelId");
        var titleProp = value.GetType().GetProperty("title");
        var contentProp = value.GetType().GetProperty("content");
        var threadIdProp = value.GetType().GetProperty("threadId");
        var messageIdProp = value.GetType().GetProperty("messageId");
        Assert.NotNull(channelIdProp);
        Assert.NotNull(titleProp);
        Assert.NotNull(contentProp);
        Assert.NotNull(threadIdProp);
        Assert.NotNull(messageIdProp);
        Assert.Equal(111111111111111111UL, channelIdProp!.GetValue(value));
        Assert.Equal("Hello", titleProp!.GetValue(value));
        Assert.Equal("World", contentProp!.GetValue(value));
        Assert.Equal(222222222222222222UL, threadIdProp!.GetValue(value));
        Assert.Equal(333333333333333333UL, messageIdProp!.GetValue(value));
    }

    [Fact]
    public async Task ForumPost_ReturnsBadGateway_OnSenderFailure()
    {
        // Arrange
        var sender = new FakeDiscordMessageSender(
            (c, ct) => Task.FromResult((true, (string?)null, (ulong?)null)),
            (channelId, title, content, tags, ct) => Task.FromResult((false, "Discord REST call failed.", (ulong?)null, (ulong?)null)));

        var controller = new ApiController(sender, NullLogger<ApiController>.Instance, new FakeSettingsProvider(new ApolloSettings()));

        // Act
        var result = await controller.CreateForumPost(111111111111111111UL, "Title", "Body", null, CancellationToken.None);

        // Assert
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, obj.StatusCode);
        var value = obj.Value!;
        var errorProp = value.GetType().GetProperty("error");
        Assert.NotNull(errorProp);
        Assert.Equal("Discord REST call failed", errorProp!.GetValue(value));
    }

    [Fact]
    public async Task ForumPost_UsesDefaults_WhenMissingTitleOrContent()
    {
        // Arrange
        string? capturedTitle = null;
        string? capturedContent = null;
        var sender = new FakeDiscordMessageSender(
            (c, ct) => Task.FromResult((true, (string?)null, (ulong?)null)),
            (channelId, title, content, tags, ct) =>
            {
                capturedTitle = title;
                capturedContent = content;
                return Task.FromResult((true, (string?)null, (ulong?)1, (ulong?)null));
            });
        var controller = new ApiController(sender, NullLogger<ApiController>.Instance, new FakeSettingsProvider(new ApolloSettings()));

        // Act
        var result = await controller.CreateForumPost(111111111111111111UL, null, null, null, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(capturedTitle);
        Assert.NotNull(capturedContent);
        Assert.Contains("Apollo forum post", capturedTitle);
        Assert.Contains("Apollo forum post created", capturedContent);
    }
}
