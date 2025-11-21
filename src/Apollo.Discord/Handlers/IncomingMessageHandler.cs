using Apollo.Core.Infrastructure;

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
    var response = await apolloAPIClient.SendMessageAsync(arg.Content);

    Console.WriteLine("Message: {0}", arg.Content);

    _ = await arg.SendAsync(response);
    return;
  }
}
