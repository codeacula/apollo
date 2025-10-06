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

public partial class ApolloChannelMenuInteractions(
    ILogger<ApolloChannelMenuInteractions> logger,
    ISettingsService settingsService,
    ISettingsProvider settingsProvider,
    IDailyAlertSetupSessionStore sessionStore) : ComponentInteractionModule<ChannelMenuInteractionContext>
{
    private readonly ILogger<ApolloChannelMenuInteractions> _logger = logger;
    private readonly ISettingsService _settingsService = settingsService;
    private readonly ISettingsProvider _settingsProvider = settingsProvider;
    private readonly IDailyAlertSetupSessionStore _sessionStore = sessionStore;

    private Task<RestMessage> RespondAsync(params IMessageComponentProperties[] components)
    {
        return ModifyResponseAsync(message =>
        {
            message.Components = components;
            message.Flags = MessageFlags.IsComponentsV2;
        });
    }

    [ComponentInteraction(ToDoChannelSelectComponent.CustomId)]
    public async Task ConfigureDailyAlertAsync()
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var selectedChannels = Context.Interaction.Data.SelectedValues;

        if (selectedChannels is not { Count: > 0 })
        {
            LogNoChannelSelected(_logger, Context.User.Id);
            await RespondAsync(
                new GeneralErrorComponent("Please select a forum channel before continuing."),
                new ToDoChannelSelectComponent());
            return;
        }

        var channelId = selectedChannels[0];

        try
        {
            var persisted = await _settingsService.SetSettingAsync(ApolloSettings.Keys.DailyAlertChannelId, channelId.ToString(CultureInfo.InvariantCulture));

            if (!persisted)
            {
                LogPersistenceFailed(_logger, channelId, Context.User.Id);
                await RespondAsync(
                    new GeneralErrorComponent("We couldn't save that forum selection. Please try again."),
                    new ToDoChannelSelectComponent());
                return;
            }

            await _settingsProvider.ReloadAsync();
        }
        catch (Exception ex)
        {
            LogPersistenceError(_logger, ex, channelId, Context.User.Id);
            await RespondAsync(
                new GeneralErrorComponent("We couldn't save that forum selection. Please try again."),
                new ToDoChannelSelectComponent());
            return;
        }

        LogPersistenceSuccess(_logger, channelId, Context.User.Id);

        await RespondAsync(
            new SuccessNotificationComponent("Daily alert forum saved", $"Daily updates will now post in <#{channelId}>."),
            new ToDoRoleSelectComponent());
    }

    [ComponentInteraction(DailyAlertSetupComponent.ChannelSelectCustomId)]
    public async Task UpdateChannelSelectionAsync()
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

        var selectedChannels = Context.Interaction.Data.SelectedValues;

        if (selectedChannels is not { Count: > 0 })
        {
            LogNoChannelSelected(_logger, userId);
            var session = await _sessionStore.GetSessionAsync(guildId.Value, userId) ?? new DailyAlertSetupSession();
            await RespondAsync(
                new GeneralErrorComponent("Please select a forum channel."),
                new DailyAlertSetupComponent(session.ChannelId, session.RoleId, session.Time, session.Message));
            return;
        }

        var channelId = selectedChannels[0];

        try
        {
            var session = await _sessionStore.GetSessionAsync(guildId.Value, userId) ?? new DailyAlertSetupSession();
            session.ChannelId = channelId;
            await _sessionStore.SetSessionAsync(guildId.Value, userId, session);

            LogChannelStaged(_logger, channelId, userId);

            await RespondAsync(new DailyAlertSetupComponent(
                session.ChannelId,
                session.RoleId,
                session.Time,
                session.Message));
        }
        catch (Exception ex)
        {
            LogSessionUpdateError(_logger, ex, userId);
            await RespondAsync(new GeneralErrorComponent("We couldn't update your selection. Please try again."));
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "No channel selected for daily alert configuration by {UserId}")]
    private static partial void LogNoChannelSelected(ILogger logger, ulong userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to persist daily alert channel {ChannelId} selected by {UserId}")]
    private static partial void LogPersistenceFailed(ILogger logger, ulong channelId, ulong userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error saving daily alert channel {ChannelId} for user {UserId}")]
    private static partial void LogPersistenceError(ILogger logger, Exception exception, ulong channelId, ulong userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Daily alert channel set to {ChannelId} by {UserId}")]
    private static partial void LogPersistenceSuccess(ILogger logger, ulong channelId, ulong userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Channel {ChannelId} staged for user {UserId}")]
    private static partial void LogChannelStaged(ILogger logger, ulong channelId, ulong userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error updating session for user {UserId}")]
    private static partial void LogSessionUpdateError(ILogger logger, Exception exception, ulong userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No guild provided for {GuildName}")]
    private static partial void LogNoGuildProvided(ILogger logger, string guildName);
}