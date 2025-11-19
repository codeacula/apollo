using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Apollo.Discord.Handlers;

public class MessageCreateHandler() : IMessageCreateGatewayHandler
{
  public async ValueTask HandleAsync(Message arg)
  {
    // This is here because when Apollo replies to the user, we get yet another MessageCreate event
    if (arg.Channel != null || arg.Author.Username == "Apollo")
    {
      return;
    }

    Console.WriteLine("Message: {0}", arg.Content);

    _ = await arg.SendAsync("Nah dog.");
    return;
  }
}
