using Apollo.Core.Conversations;
using Apollo.Core.Infrastructure.API;
using Apollo.Core.Infrastructure.Cache;
using Apollo.Core.Logging;
using Apollo.Discord.Config;
using Apollo.Domain.Users.ValueObjects;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Apollo.Discord.Handlers;

public class IncomingMessageHandler(
  IApolloAPIClient apolloAPIClient,
  IUserCache userCache,
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
    // var validationResult = await userCache.GetUserAccessAsync(username);
    // if (validationResult.IsFailed || validationResult is null)
    // {
    //   ValidationLogs.ValidationFailed(logger, username, string.Join(", ", validationResult?.Errors.Select(e => e.Message) ?? []));
    //   _ = await arg.SendAsync("Sorry, unable to verify your access at this time.");
    //   return;
    // }

    // if (validationResult.Value is not null && !validationResult.Value.Value)
    // {
    //   ValidationLogs.AccessDenied(logger, username);
    //   _ = await arg.SendAsync("Sorry, you do not have access to use this bot.");
    //   return;
    // }

    // Send request to API
    try
    {
      var newMessage = new NewMessage
      {
        Username = username,
        Content = arg.Content
      };

      var response = await apolloAPIClient.SendMessageAsync(newMessage);

      if (response.IsFailed)
      {
        _ = await arg.SendAsync($"An error occurred:\n{string.Join("\n", response.Errors.Select(e => e.Message))}");
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
