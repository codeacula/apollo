using Apollo.AI;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Apollo.Discord.Handlers;

public class MessageCreateHandler(IApolloAIAgent apolloAIAgent) : IMessageCreateGatewayHandler
{
  public async ValueTask HandleAsync(Message arg)
  {
    // This is here because when Apollo replies to the user, we get yet another MessageCreate event
    if (arg.Author.IsBot || arg.Channel != null || arg.Author.Username == "Apollo")
    {
      return;
    }

    var response = await apolloAIAgent.ChatAsync(arg.Author.Username, arg.Content);

    _ = await arg.SendAsync(response);
    return;
  }
}
