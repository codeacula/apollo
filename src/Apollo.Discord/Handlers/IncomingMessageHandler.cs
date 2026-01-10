using System.Globalization;

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
  IPersonService personService,
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

    // Resolve PlatformId to PersonId
    var platformId = new PlatformId(arg.Author.Username, arg.Author.Id.ToString(CultureInfo.InvariantCulture), ApolloPlatform.Discord);
    var personIdResult = await personCache.GetPersonIdAsync(platformId);

    PersonId personId;
    if (personIdResult.IsFailed)
    {
      ValidationLogs.ValidationFailed(logger, platformId.PlatformUserId, personIdResult.GetErrorMessages());
      _ = await arg.SendAsync("Sorry, unable to verify your access at this time.");
      return;
    }

    if (personIdResult.Value.HasValue)
    {
      personId = personIdResult.Value.Value;
    }
    else
    {
      // PersonId not in cache, get or create via service
      var getOrCreateResult = await personService.GetOrCreateAsync(platformId);
      if (getOrCreateResult.IsFailed)
      {
        ValidationLogs.ValidationFailed(logger, platformId.PlatformUserId, getOrCreateResult.GetErrorMessages());
        _ = await arg.SendAsync("Sorry, unable to verify your access at this time.");
        return;
      }

      personId = getOrCreateResult.Value.Id;

      // Cache the mapping for future lookups
      _ = await personService.MapPlatformIdToPersonIdAsync(platformId, personId);
    }

    // Validate user access using PersonId
    var validationResult = await personCache.GetAccessAsync(personId);

    if (validationResult.IsFailed || !(validationResult.Value ?? false))
    {
      ValidationLogs.ValidationFailed(logger, platformId.PlatformUserId, validationResult.GetErrorMessages());
      _ = await arg.SendAsync("Sorry, unable to verify your access at this time.");
      return;
    }

    try
    {
      var newMessage = new NewMessageRequest
      {
        Username = arg.Author.Username,
        Content = arg.Content,
        Platform = ApolloPlatform.Discord,
        PlatformUserId = platformId.PlatformUserId
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
      DiscordLogs.MessageProcessingFailed(logger, platformId.PlatformUserId, arg.Author.Username, ex.Message, ex);
      _ = await arg.SendAsync("Sorry, an unexpected error occurred while processing your message. Please try again later.");
      return;
    }
  }
}
