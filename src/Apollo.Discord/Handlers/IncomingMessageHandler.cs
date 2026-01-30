using Apollo.Core.Conversations;
using Apollo.Core.Logging;
using Apollo.Core.People;
using Apollo.Domain.People.ValueObjects;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

using ApolloPlatform = Apollo.Domain.Common.Enums.Platform;

namespace Apollo.Discord.Handlers;

public class IncomingMessageHandler(
  IApolloServiceClient apolloServiceClient,
  IPersonCache personCache,
  ILogger<IncomingMessageHandler> logger) : IMessageCreateGatewayHandler
{
  public async ValueTask HandleAsync(Message arg)
  {
    // This is here because when Apollo replies to the user, we get yet another MessageCreate event
    if (arg.GuildId != null || arg.Author.IsBot)
    {
      return;
    }

    // Resolve PlatformId to PersonId
    var platformId = new PlatformId(arg.Author.Username, arg.Author.Id.ToString(CultureInfo.InvariantCulture), ApolloPlatform.Discord);

    // Validate user access using PersonId
    var validationResult = await personCache.GetAccessAsync(platformId);

    if (validationResult.IsFailed)
    {
      ValidationLogs.ValidationFailed(logger, platformId.PlatformUserId, validationResult.GetErrorMessages());
      _ = await arg.SendAsync("Sorry, unable to verify your access at this time.");
      return;
    }

     // If we have a cached value of false, deny access. Cache miss (null) allows the request to proceed
     // to the API which will validate and update the cache as needed.
     if (validationResult.Value is false)
    {
      ValidationLogs.ValidationFailed(logger, platformId.PlatformUserId, "Access denied");
      _ = await arg.SendAsync("Sorry, you do not have access to Apollo.");
      return;
    }

    try
    {
      var newMessage = new NewMessageRequest
      {
        PlatformId = platformId,
        Content = arg.Content
      };

      var response = await apolloServiceClient.SendMessageAsync(newMessage, CancellationToken.None);

      if (response.IsFailed)
      {
        _ = await arg.SendAsync($"An error occurred:\n{response.GetErrorMessages("\n")}");
        return;
      }

      _ = await arg.SendAsync(response.Value);
    }
    catch (Exception ex)
    {
      DiscordLogs.MessageProcessingFailed(logger, arg.Author.Username, platformId.PlatformUserId, ex.Message, ex);
      _ = await arg.SendAsync("Sorry, an unexpected error occurred while processing your message. Please try again later.");
      return;
    }
  }
}
