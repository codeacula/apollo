using Apollo.Core;
using Apollo.Core.API;
using Apollo.Core.Conversations;
using Apollo.Core.Logging;
using Apollo.Core.People;
using Apollo.Discord.Config;
using Apollo.Domain.People.ValueObjects;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

using ApolloPlatform = Apollo.Domain.Common.Enums.Platform;

namespace Apollo.Discord.Handlers;

public class IncomingMessageHandler(
  IApolloAPIClient apolloAPIClient,
  IPersonCache personCache,
  DiscordConfig discordConfig,
  ILogger<IncomingMessageHandler> logger) : IMessageCreateGatewayHandler
{
  public async ValueTask HandleAsync(Message arg)
  {
    // This is here because when Apollo replies to the user, we get yet another MessageCreate event
    if (arg.GuildId != null || arg.Author.Username == discordConfig.BotName)
    {
      return;
    }

    // Validate user access
    var username = new Username(arg.Author.Username, ApolloPlatform.Discord);
    var validationResult = await personCache.GetAccessAsync(username);
    if (validationResult.IsFailed)
    {
      ValidationLogs.ValidationFailed(logger, username, validationResult.GetErrorMessages());
      _ = await arg.SendAsync("Sorry, unable to verify your access at this time.");
      return;
    }

    if (!validationResult.Value ?? false)
    {
      ValidationLogs.AccessDenied(logger, username);
      _ = await arg.SendAsync("Sorry, you do not have access to use this bot.");
      return;
    }

    // Send request to API
    try
    {
      var discordUserId = arg.Author.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
      var newMessage = new NewMessage
      {
        Username = username,
        Content = arg.Content,
        Platform = ApolloPlatform.Discord,
        PlatformIdentifier = discordUserId
      };

      var response = await apolloAPIClient.SendMessageAsync(newMessage);

      if (response.IsFailed)
      {
        _ = await arg.SendAsync($"An error occurred:\n{response.GetErrorMessages("\n")}");
        return;
      }

      _ = await arg.SendAsync(response.Value);
    }
    catch (Exception ex)
    {
      DiscordLogs.MessageProcessingFailed(logger, username.Value, ex.Message, ex);
      _ = await arg.SendAsync("Sorry, an unexpected error occurred while processing your message. Please try again later.");
      return;
    }
  }
}
