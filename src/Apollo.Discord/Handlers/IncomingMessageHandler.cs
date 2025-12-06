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
    if (validationResult.IsFailed || validationResult is null)
    {
      ValidationLogs.ValidationFailed(logger, username, string.Join(", ", validationResult?.Errors.Select(e => e.Message) ?? []));
      _ = await arg.SendAsync("Sorry, unable to verify your access at this time.");
      return;
    }

    if (validationResult.Value is not null && !validationResult.Value.Value)
    {
      ValidationLogs.AccessDenied(logger, username);
      _ = await arg.SendAsync("Sorry, you do not have access to use this bot.");
      return;
    }

    // Send request to API
    try
    {
      var newMessage = new NewMessage
      {
        Username = username,
        Content = arg.Content,
        Platform = ApolloPlatform.Discord
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
