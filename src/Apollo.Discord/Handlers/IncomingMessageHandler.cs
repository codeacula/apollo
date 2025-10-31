using Apollo.AI;
using Apollo.Database;
using Apollo.Database.Repository;

using Microsoft.SemanticKernel.ChatCompletion;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Apollo.Discord.Handlers;

public class MessageCreateHandler(IApolloAIAgent apolloAIAgent, IApolloUserRepo apolloUserRepo) : IMessageCreateGatewayHandler
{
  public async ValueTask HandleAsync(Message arg)
  {
    // This is here because when Apollo replies to the user, we get yet another MessageCreate event
    if (arg.Channel != null || arg.Author.Username == "Apollo")
    {
      return;
    }

    var user = await apolloUserRepo.GetOrCreateApolloUserAsync(arg.Author.Username) ?? throw new InvalidOperationException("Failed to get or create Apollo user.");

    if (!user.HasAccess)
    {
      _ = await arg.SendAsync("You do not have access to Apollo. Please contact an administrator.");
      return;
    }

    var userChats = await apolloUserRepo.GetUserChatsAsync(user.Id);

    var response = await apolloAIAgent.ChatAsync(arg.Author.Username, arg.Content);

    var chatHistory = new ChatHistory();

    foreach (var chat in userChats)
    {
      if (chat.Outgoing)
      {
        chatHistory.AddUserMessage(chat.ChatText);
        continue;
      }

      chatHistory.AddAssistantMessage(chat.ChatText);
    }

    _ = await arg.SendAsync(response);
    return;
  }
}
