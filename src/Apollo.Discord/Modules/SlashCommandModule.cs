using Apollo.Core.API;

using Microsoft.AspNetCore.Authentication;

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

  [SlashCommand("todo", "Quickly create a new To Do")]
  public async Task CreateFastToDoAsync([SlashCommandParameter(Name = "todo", Description = "The To Do you wish to create.")] string todo)
  {
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    apolloAPIClient.SendMessageAsync()
  }

  [SlashCommand("new", "Shows a form to create a new To Do")]
  public async Task CreateToDoAsync()
  {
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    var container = new ComponentContainerProperties
    {
      AccentColor = new Color(0x3B5BA5),

      Components = [
        new TextDisplayProperties("# New To Do"),
        new TextDisplayProperties("### Priority"),
        new StringMenuProperties("priority")
        {
          new("Green", "green") { Emoji = EmojiProperties.Standard("🟢"), Description = "Low Energy", Default = true },
          new("Yellow", "yellow") { Emoji = EmojiProperties.Standard("🟡"), Description = "Medium Energy" },
          new("Red", "red") { Emoji = EmojiProperties.Standard("🔴"), Description = "High Energy" },
        },
        new TextDisplayProperties("### Energy Level"),
        new StringMenuProperties("energy")
        {
          new("Green", "green") { Emoji = EmojiProperties.Standard("🟢"), Description = "Low Energy", Default = true },
          new("Yellow", "yellow") { Emoji = EmojiProperties.Standard("🟡"), Description = "Medium Energy" },
          new("Red", "red") { Emoji = EmojiProperties.Standard("🔴"), Description = "High Energy" },
        }
      ]
    };

    _ = await ModifyResponseAsync(message =>
    {
      message.Components = [container];
      message.Flags = MessageFlags.IsComponentsV2;
    });
  }
}
