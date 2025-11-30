using Apollo.Core.Infrastructure.API;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Apollo.Discord.Handlers;

public class MessageCreateHandler(IApolloAPIClient apolloAPIClient) : IMessageCreateGatewayHandler
{
  public async ValueTask HandleAsync(Message arg)
  {
    // This is here because when Apollo replies to the user, we get yet another MessageCreate event
    if (arg.Channel != null || arg.Author.Username == "Apollo")
    {
      return;
    }

    // Check redis cache here to see if they have permission to use this

    // Send request to API
    try
    {
      var response = await apolloAPIClient.SendMessageAsync(arg.Content);

      if (!response.IsSuccess)
      {
        Console.WriteLine("Failed to get response from Apollo API: {0}", response.Error);
        _ = await arg.SendAsync("Sorry, something went wrong while processing your message.");
        return;
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Failed to send initial processing message: {0}", ex);
      _ = await arg.SendAsync($"Exception occurred while processing your message. {ex.Message}");
      return;
    }
  }
}
