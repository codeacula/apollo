using System.Globalization;
using Apollo.Core.Configuration;
using Apollo.Core.Services;
using Apollo.Discord.Components;
using Apollo.Discord.Services;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace Apollo.Discord.Modules;

public partial class ApolloButtonInteractions(
    ILogger<ApolloButtonInteractions> logger,
    IDailyAlertSetupSessionStore sessionStore,
    ISettingsService settingsService,
    ISettingsProvider settingsProvider) : ComponentInteractionModule<ButtonInteractionContext>
{
    private readonly ILogger<ApolloButtonInteractions> _logger = logger;
    private readonly IDailyAlertSetupSessionStore _sessionStore = sessionStore;
    private readonly ISettingsService _settingsService = settingsService;
    private readonly ISettingsProvider _settingsProvider = settingsProvider;

    private Task<RestMessage> RespondAsync(params IMessageComponentProperties[] components)
    {
        return ModifyResponseAsync(message =>
        {
            message.Components = components;
            message.Flags = MessageFlags.IsComponentsV2;
        });
    }

    [ComponentInteraction(DailyAlertTimeConfigComponent.ButtonCustomId)]
    public async Task ShowDailyAlertTimeConfigModalAsync()
    {
        LogShowModal(_logger, Context.User.Id);
        await RespondAsync(InteractionCallback.Modal(new DailyAlertTimeConfigModal()));
    }

    [ComponentInteraction(DailyAlertSetupComponent.ConfigureTimeButtonCustomId)]
    public async Task ShowUnifiedTimeConfigModalAsync()
    {
        var guildId = Context.Guild?.Id;
        var userId = Context.User.Id;

        if (!guildId.HasValue)
        {
            return;
        }

        var session = await _sessionStore.GetSessionAsync(guildId.Value, userId);
        var modal = new DailyAlertTimeConfigModal(session?.Time, session?.Message);

        LogShowModal(_logger, userId);
        await RespondAsync(InteractionCallback.Modal(modal));
    }

    [ComponentInteraction(DailyAlertSetupComponent.SaveButtonCustomId)]
    public async Task SaveConfigurationAsync()
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var guildId = Context.Guild?.Id;
        var userId = Context.User.Id;

        if (!guildId.HasValue)
        {
            LogNoGuildProvided(_logger, Context.User.Username);
            await RespondAsync(new GeneralErrorComponent("No guild provided."));
            return;
        }

        var session = await _sessionStore.GetSessionAsync(guildId.Value, userId);

        if (session is null || !session.ChannelId.HasValue || !session.RoleId.HasValue ||
            string.IsNullOrWhiteSpace(session.Time) || string.IsNullOrWhiteSpace(session.Message))
        {
            LogIncompleteConfiguration(_logger, userId);
            await RespondAsync(
                new GeneralErrorComponent("Configuration is incomplete. Please fill in all fields."),
                new DailyAlertSetupComponent(session?.ChannelId, session?.RoleId, session?.Time, session?.Message));
            return;
        }

        try
        {
            var channelPersisted = await _settingsService.SetSettingAsync(
                ApolloSettings.Keys.DailyAlertChannelId,
                session.ChannelId.Value.ToString(CultureInfo.InvariantCulture));

            var rolePersisted = await _settingsService.SetSettingAsync(
                ApolloSettings.Keys.DailyAlertRoleId,
                session.RoleId.Value.ToString(CultureInfo.InvariantCulture));

            var timePersisted = await _settingsService.SetSettingAsync(
                ApolloSettings.Keys.DailyAlertTime,
                session.Time);

            var messagePersisted = await _settingsService.SetSettingAsync(
                ApolloSettings.Keys.DailyAlertInitialMessage,
                session.Message);

            if (!channelPersisted || !rolePersisted || !timePersisted || !messagePersisted)
            {
                LogSaveFailed(_logger, userId);
                await RespondAsync(
                    new GeneralErrorComponent("We couldn't save your configuration. Please try again."),
                    new DailyAlertSetupComponent(session.ChannelId, session.RoleId, session.Time, session.Message));
                return;
            }

            await _settingsProvider.ReloadAsync();
            await _sessionStore.DeleteSessionAsync(guildId.Value, userId);

            LogSaveSuccess(_logger, userId, session.ChannelId.Value, session.RoleId.Value, session.Time);

            await RespondAsync(
                new SuccessNotificationComponent(
                    "Daily alert configuration saved",
                    $"Daily updates will now be posted in <#{session.ChannelId.Value}> at **{session.Time}**, notifying <@&{session.RoleId.Value}>."));
        }
        catch (Exception ex)
        {
            LogSaveError(_logger, ex, userId);
            await RespondAsync(
                new GeneralErrorComponent("We couldn't save your configuration. Please try again."),
                new DailyAlertSetupComponent(session.ChannelId, session.RoleId, session.Time, session.Message));
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Showing daily alert time config modal for user {UserId}")]
    private static partial void LogShowModal(ILogger logger, ulong userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No guild provided for {GuildName}")]
    private static partial void LogNoGuildProvided(ILogger logger, string guildName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Incomplete configuration for user {UserId}")]
    private static partial void LogIncompleteConfiguration(ILogger logger, ulong userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to save configuration for user {UserId}")]
    private static partial void LogSaveFailed(ILogger logger, ulong userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Configuration saved for user {UserId}: channel {ChannelId}, role {RoleId}, time {Time}")]
    private static partial void LogSaveSuccess(ILogger logger, ulong userId, ulong channelId, ulong roleId, string time);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error saving configuration for user {UserId}")]
    private static partial void LogSaveError(ILogger logger, Exception exception, ulong userId);
}
