using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Rydia.Discord.Components;

namespace Rydia.Discord.Modules;

public partial class RydiaChannelMenuInteractions(ILogger<RydiaChannelMenuInteractions> logger) : ComponentInteractionModule<ChannelMenuInteractionContext>
{
    private readonly ILogger<RydiaChannelMenuInteractions> _logger = logger;

    private Task<RestMessage> RespondAsync(IMessageComponentProperties component)
    {
        return ModifyResponseAsync(message =>
        {
            message.Components = [component];
            message.Flags = MessageFlags.IsComponentsV2;
        });
    }

    [ComponentInteraction(ToDoChannelSelectComponent.CustomId)]
    public async Task ConfigureDailyAlertAsync()
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        await RespondAsync(new ToDoChannelSelectComponent());
    }
}