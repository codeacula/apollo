using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Rydia.DiscordModules;

public class ToDoModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("configure-daily-alert", "Set up which forum daily alerts are posted to.")]
    public async Task ConfigureDailyAlertAsync()
    {
        return RespondAsync(message);
    }
}