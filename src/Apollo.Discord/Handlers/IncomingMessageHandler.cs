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
  IApolloServiceClient apolloServiceClient,
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
    var personId = new PersonId(ApolloPlatform.Discord, arg.Author.Id.ToString(CultureInfo.InvariantCulture));
    var validationResult = await personCache.GetAccessAsync(personId);

    if (validationResult.IsFailed || !(validationResult.Value ?? false))
    {
      ValidationLogs.ValidationFailed(logger, personId.Value, validationResult.GetErrorMessages());
      _ = await arg.SendAsync("Sorry, unable to verify your access at this time.");
      return;
    }

    // Send request to API
    try
    {
      var newMessage = new NewMessageRequest
      {
        Username = arg.Author.Username,
        Content = arg.Content,
        Platform = ApolloPlatform.Discord,
        ProviderId = personId.ProviderId
      };

      var response = await apolloServiceClient.SendMessageAsync(newMessage);

      if (response.IsFailed)
      {
        _ = await arg.SendAsync($"An error occurred:\n{response.GetErrorMessages("\n")}");
        return;
      }

      _ = await arg.SendAsync(response.Value);
    }
    catch (Exception ex)
    {
      DiscordLogs.MessageProcessingFailed(logger, arg.Author.Username, ex.Message, ex);
      _ = await arg.SendAsync("Sorry, an unexpected error occurred while processing your message. Please try again later.");
      return;
    }
  }
}
