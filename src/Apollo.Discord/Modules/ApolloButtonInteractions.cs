using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Apollo.Discord.Components;

namespace Apollo.Discord.Modules;

public partial class ApolloButtonInteractions(ILogger<ApolloButtonInteractions> logger) : ComponentInteractionModule<ButtonInteractionContext>
{
    private readonly ILogger<ApolloButtonInteractions> _logger = logger;

    [ComponentInteraction(DailyAlertTimeConfigComponent.ButtonCustomId)]
    public async Task ShowDailyAlertTimeConfigModalAsync()
    {
        LogShowModal(_logger, Context.User.Id);
        await RespondAsync(InteractionCallback.Modal(new DailyAlertTimeConfigModal()));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Showing daily alert time config modal for user {UserId}")]
    private static partial void LogShowModal(ILogger logger, ulong userId);
}
