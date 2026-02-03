using Apollo.Core.Conversations;
using Apollo.Core.Logging;
using Apollo.Core.People;
using Apollo.Discord.Extensions;

using FluentResults;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Apollo.Discord.Handlers;

public sealed class IncomingMessageHandler(
  IApolloServiceClient apolloServiceClient,
  IPersonCache personCache,
  ILogger<IncomingMessageHandler> logger) : IMessageCreateGatewayHandler
{
  private async Task AccessDeniedAsync(Message arg)
  {
    ValidationLogs.ValidationFailed(logger, arg.GetDiscordPlatformId().PlatformUserId, "Access denied");
    _ = await arg.SendAsync("Sorry, you do not have access to Apollo.");
  }

  public async ValueTask HandleAsync(Message arg)
  {
    if (!IsDirectMessage(arg))
    {
      return;
    }

    var validationResult = await personCache.GetAccessAsync(arg.GetDiscordPlatformId());

    if (validationResult.IsFailed)
    {
      await ValidationFailedAsync(arg, validationResult);
      return;
    }

    if (validationResult.Value is false)
    {
      await AccessDeniedAsync(arg);
      return;
    }

    var content = arg.Content;
    await SendToServiceAsync(content, arg);
  }

  private static bool IsDirectMessage(Message message)
  {
    return message.GuildId == null && !message.Author.IsBot;
  }

  private async Task SendToServiceAsync(string content, Message arg)
  {
    var platformId = arg.GetDiscordPlatformId();

    try
    {
      var newMessage = new NewMessageRequest
      {
        PlatformId = platformId,
        Content = content
      };

      var response = await apolloServiceClient.SendMessageAsync(newMessage, CancellationToken.None);

      if (response.IsFailed)
      {
        _ = await arg.SendAsync($"An error occurred:\n{response.GetErrorMessages("\n")}");
        return;
      }

      _ = await arg.SendAsync(response.Value);
    }
    catch (Exception ex)
    {
      DiscordLogs.MessageProcessingFailed(logger, arg.Author.Username, platformId.PlatformUserId, ex.Message, ex);
      _ = await arg.SendAsync("Sorry, an unexpected error occurred while processing your message. Please try again later.");
    }
  }

  private async Task ValidationFailedAsync(Message arg, Result<bool?> validationResult)
  {
    ValidationLogs.ValidationFailed(logger, arg.GetDiscordPlatformId().PlatformUserId, validationResult.GetErrorMessages());
    _ = await arg.SendAsync("Sorry, unable to verify your access at this time.");
  }
}
