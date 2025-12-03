using Apollo.Core.Conversations;
using Apollo.Core.Infrastructure.API;
using Apollo.Core.Infrastructure.Services;
using Apollo.Core.Logging;
using Apollo.Discord.Config;
using Apollo.Domain.ValueObjects;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Apollo.Discord.Handlers;

public class IncomingMessageHandler(
  IApolloAPIClient apolloAPIClient,
  IUserValidationService userValidationService,
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
    var username = new Username(arg.Author.Username);
    var validationResult = await userValidationService.ValidateUserAccessAsync(username);
    if (validationResult.IsFailed)
    {
      ValidationLogs.ValidationFailed(logger, username.Value, string.Join(", ", validationResult.Errors.Select(e => e.Message)));
      _ = await arg.SendAsync("Sorry, unable to verify your access at this time.");
      return;
    }

    if (!validationResult.Value)
    {
      ValidationLogs.AccessDenied(logger, username.Value);
      _ = await arg.SendAsync("Sorry, you do not have access to use this bot.");
      return;
    }

    // Send request to API
    try
    {
      var newMessage = new NewMessage
      {
        Username = arg.Author.Username,
        Content = arg.Content
      };

      var response = await apolloAPIClient.SendMessageAsync(newMessage);

      if (response.IsFailed)
      {
        _ = await arg.SendAsync("Sorry, something went wrong while processing your message.");
        return;
      }

      _ = await arg.SendAsync(response.Value);
    }
    catch (Exception ex)
    {
      _ = await arg.SendAsync($"Exception occurred while processing your message. {ex.Message}");
      return;
    }
  }
}
