using System.Text;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Rydia.DiscordModules;

public partial class ToDoModule(ILogger<ToDoModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<ToDoModule> _logger = logger;


    [SlashCommand("configure-daily-alert", "Set up which forum daily alerts are posted to.")]
    public async Task ConfigureDailyAlertAsync()
    {
        LogStartConfigure(_logger, Context.User.Username);
        await RespondAsync(InteractionCallback.DeferredMessage());

        var responseBuilder = new StringBuilder();

        responseBuilder.AppendLine("# HERRO");

        if (Context.Guild is null)
        {
            throw new Exception("Unable");
        }

        Context.Guild.Channels.Select(x => x.Value.Name).ToList().ForEach(x => responseBuilder.AppendLine(x));

        var container = new ComponentContainerProperties
        {
            AccentColor = new Color(0x3B5BA5),
            Components = [
                new TextDisplayProperties(responseBuilder.ToString())
            ]
        };

        _ = await ModifyResponseAsync(message =>
        {
            message.Components = [container, container];
            message.Flags = MessageFlags.IsComponentsV2;
        });
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "configure-daily-alert initialized by {Username}"
    )]
    public static partial void LogStartConfigure(ILogger logger, string username);
}