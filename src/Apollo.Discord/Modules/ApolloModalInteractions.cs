using System.Globalization;
using System.Text.RegularExpressions;
using Apollo.Core.Configuration;
using Apollo.Core.Services;
using Apollo.Discord.Components;
using Apollo.Discord.Services;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace Apollo.Discord.Modules;

public partial class ApolloModalInteractions(
    ILogger<ApolloModalInteractions> logger,
    ISettingsService settingsService,
    ISettingsProvider settingsProvider,
    IDailyAlertSetupSessionStore sessionStore) : ComponentInteractionModule<ModalInteractionContext>
{
    private readonly ILogger<ApolloModalInteractions> _logger = logger;
    private readonly ISettingsService _settingsService = settingsService;
    private readonly ISettingsProvider _settingsProvider = settingsProvider;
    private readonly IDailyAlertSetupSessionStore _sessionStore = sessionStore;
    private static readonly Regex TimeFormatRegex = new(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$", RegexOptions.Compiled);

    private Task<RestMessage> RespondAsync(params IMessageComponentProperties[] components)
    {
        return ModifyResponseAsync(message =>
        {
            message.Components = components;
            message.Flags = MessageFlags.IsComponentsV2;
        });
    }

    [ComponentInteraction(DailyAlertTimeConfigModal.CustomId)]
    public async Task ConfigureDailyAlertTimeAsync()
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var guildId = Context.Guild?.Id;
        var userId = Context.User.Id;

        var components = Context.Interaction.Data.Components;

        var timeInput = components
            .OfType<TextInput>()
            .FirstOrDefault(c => c.CustomId == DailyAlertTimeConfigModal.TimeInputCustomId)
            ?.Value?.Trim();

        var messageInput = components
            .OfType<TextInput>()
            .FirstOrDefault(c => c.CustomId == DailyAlertTimeConfigModal.MessageInputCustomId)
            ?.Value?.Trim();

        if (string.IsNullOrWhiteSpace(timeInput))
        {
            timeInput = DailyAlertTimeConfigModal.DefaultTime;
        }

        if (string.IsNullOrWhiteSpace(messageInput))
        {
            messageInput = DailyAlertTimeConfigModal.DefaultMessage;
        }

        if (!TimeFormatRegex.IsMatch(timeInput))
        {
            LogInvalidInput(_logger, userId, $"Invalid time format: {timeInput}");

            if (guildId.HasValue)
            {
                var sessionForError = await _sessionStore.GetSessionAsync(guildId.Value, userId) ?? new DailyAlertSetupSession();
                await RespondAsync(
                    new GeneralErrorComponent("Invalid time format. Please use HH:mm format (e.g., 06:00 or 14:30)."),
                    new DailyAlertSetupComponent(sessionForError.ChannelId, sessionForError.RoleId, sessionForError.Time, sessionForError.Message));
            }
            else
            {
                await RespondAsync(
                    new GeneralErrorComponent("Invalid time format. Please use HH:mm format (e.g., 06:00 or 14:30)."),
                    new DailyAlertTimeConfigComponent());
            }
            return;
        }

        if (guildId.HasValue)
        {
            try
            {
                var session = await _sessionStore.GetSessionAsync(guildId.Value, userId) ?? new DailyAlertSetupSession();
                session.Time = timeInput;
                session.Message = messageInput;
                await _sessionStore.SetSessionAsync(guildId.Value, userId, session);

                LogTimeAndMessageStaged(_logger, userId, timeInput);

                await RespondAsync(new DailyAlertSetupComponent(
                    session.ChannelId,
                    session.RoleId,
                    session.Time,
                    session.Message));
            }
            catch (Exception ex)
            {
                LogSessionUpdateError(_logger, ex, userId);
                await RespondAsync(new GeneralErrorComponent("We couldn't update your configuration. Please try again."));
            }
        }
        else
        {
            try
            {
                var timePersisted = await _settingsService.SetSettingAsync(ApolloSettings.Keys.DailyAlertTime, timeInput);
                var messagePersisted = await _settingsService.SetSettingAsync(ApolloSettings.Keys.DailyAlertInitialMessage, messageInput);

                if (!timePersisted || !messagePersisted)
                {
                    LogPersistenceFailed(_logger, userId);
                    await RespondAsync(
                        new GeneralErrorComponent("We couldn't save your configuration. Please try again."),
                        new DailyAlertTimeConfigComponent());
                    return;
                }

                await _settingsProvider.ReloadAsync();
                LogPersistenceSuccess(_logger, timeInput, userId);

                await RespondAsync(
                    new SuccessNotificationComponent(
                        "Daily alert configuration complete",
                        $"Daily updates will now be posted at **{timeInput}** with your custom message."));
            }
            catch (Exception ex)
            {
                LogPersistenceError(_logger, ex, userId);
                await RespondAsync(
                    new GeneralErrorComponent("We couldn't save your configuration. Please try again."),
                    new DailyAlertTimeConfigComponent());
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid input for daily alert time config by {UserId}: {Reason}")]
    private static partial void LogInvalidInput(ILogger logger, ulong userId, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to persist daily alert time config for user {UserId}")]
    private static partial void LogPersistenceFailed(ILogger logger, ulong userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error saving daily alert time config for user {UserId}")]
    private static partial void LogPersistenceError(ILogger logger, Exception exception, ulong userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Daily alert time set to {Time} by {UserId}")]
    private static partial void LogPersistenceSuccess(ILogger logger, string time, ulong userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Time {Time} and message staged for user {UserId}")]
    private static partial void LogTimeAndMessageStaged(ILogger logger, ulong userId, string time);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error updating session for user {UserId}")]
    private static partial void LogSessionUpdateError(ILogger logger, Exception exception, ulong userId);
}
