using Apollo.Core.Conversations;
using Apollo.Core.Infrastructure.API;
using Apollo.Discord.Config;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Apollo.Discord.Handlers;

public class MessageCreateHandler(IApolloAPIClient apolloAPIClient, DiscordConfig discordConfig) : IMessageCreateGatewayHandler
{
  public async ValueTask HandleAsync(Message arg)
  {
    // This is here because when Apollo replies to the user, we get yet another MessageCreate event
    if (arg.GuildId != null || arg.Author.Username == discordConfig.BotName)
    {
      return;
    }

    // Check redis cache here to see if they have permission to use this

    // Send request to API
    try
    {
      var newMessage = new NewMessage(arg.Author.Username, arg.Content);
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
