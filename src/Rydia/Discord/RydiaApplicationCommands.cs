using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Rydia.Discord.Components;

namespace Rydia.Discord.Modules;

public partial class RydiaApplicationCommands(ILogger<RydiaApplicationCommands> logger) : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<RydiaApplicationCommands> _logger = logger;

    private Task<RestMessage> RespondAsync(IMessageComponentProperties component)
    {
        return ModifyResponseAsync(message =>
        {
            message.Components = [component];
            message.Flags = MessageFlags.IsComponentsV2;
        });
    }


    [SlashCommand("configure-daily-alert", "Set up which forum daily alerts are posted to.")]
    public async Task ConfigureDailyAlertAsync()
    {
        LogStartConfigure(_logger, Context.User.Username);
        await RespondAsync(InteractionCallback.DeferredMessage());

        if (Context.Guild is null)
        {
            LogNoGuildProvided(_logger, Context.User.Username);
            await RespondAsync(new GeneralErrorComponent("No guild provided."));
            return;
        }

        await RespondAsync(new ToDoChannelSelectComponent());
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "configure-daily-alert initialized by {Username}"
    )]
    public static partial void LogStartConfigure(ILogger logger, string username);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "No guild provided for {GuildName}"
    )]
    public static partial void LogNoGuildProvided(ILogger logger, string guildName);
}