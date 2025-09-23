using System.Text;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Rydia.Discord.Modules;

public partial class RydiaApplicationCommands(ILogger<RydiaApplicationCommands> logger) : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<RydiaApplicationCommands> _logger = logger;


    [SlashCommand("configure-daily-alert", "Set up which forum daily alerts are posted to.")]
    public async Task ConfigureDailyAlertAsync()
    {
        LogStartConfigure(_logger, Context.User.Username);
        await RespondAsync(InteractionCallback.DeferredMessage());

        if (Context.Guild is null)
        {
            LogNoGuildProvided(_logger, Context.User.Username);
            return;
        }

        var responseBuilder = new StringBuilder("# Channels Available For Daily Updates\nSelect which forum channel you would like daily updates to be posted in.\n\n");

        var channelMenuProperties = new ChannelMenuProperties("channel_select")
        {
            ChannelTypes = [ChannelType.ForumGuildChannel]
        };

        var container = new ComponentContainerProperties
        {
            AccentColor = new Color(0x3B5BA5),
            Components = [
                new TextDisplayProperties(responseBuilder.ToString()),
                channelMenuProperties
            ]
        };

        _ = await ModifyResponseAsync(message =>
        {
            message.Components = [container];
            message.Flags = MessageFlags.IsComponentsV2;
        });
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