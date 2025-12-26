using Apollo.Core.API;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Apollo.Discord.Modules;

public class SlashCommandModule(IApolloAPIClient apolloAPIClient) : ApplicationCommandModule<ApplicationCommandContext>
{
  [SlashCommand("config", "Allows you to configure your Apollo settings.")]
  public async Task ConfigAsync()
  {
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    // Fetch user & settings

    // Display settings to the user
    var container = new ComponentContainerProperties
    {
      AccentColor = new Color(0x3B5BA5),

      Components = [
        new TextDisplayProperties("### Your current settings will be displayed here."),
      ]
    };

    _ = await ModifyResponseAsync(message =>
    {
      message.Components = [container];
      message.Flags = MessageFlags.IsComponentsV2;
    });
  }
}
