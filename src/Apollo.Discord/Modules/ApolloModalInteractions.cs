using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Apollo.Core.Configuration;
using Apollo.Core.Services;
using Apollo.Discord.Components;

namespace Apollo.Discord.Modules;

public partial class ApolloModalInteractions(
    ILogger<ApolloModalInteractions> logger,
    ISettingsService settingsService,
    ISettingsProvider settingsProvider) : ComponentInteractionModule<ModalInteractionContext>
{
    private readonly ILogger<ApolloModalInteractions> _logger = logger;
    private readonly ISettingsService _settingsService = settingsService;
    private readonly ISettingsProvider _settingsProvider = settingsProvider;
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

        var components = Context.Interaction.Data.Components;

        // Extract time input
        var timeInput = components
            .OfType<TextInput>()
            .FirstOrDefault(c => c.CustomId == DailyAlertTimeConfigModal.TimeInputCustomId)
            ?.Value?.Trim();

        // Extract message input
        var messageInput = components
            .OfType<TextInput>()
            .FirstOrDefault(c => c.CustomId == DailyAlertTimeConfigModal.MessageInputCustomId)
            ?.Value?.Trim();

        // Use default values if inputs are empty
        if (string.IsNullOrWhiteSpace(timeInput))
        {
            timeInput = DailyAlertTimeConfigModal.DefaultTime;
        }

        if (string.IsNullOrWhiteSpace(messageInput))
        {
            messageInput = DailyAlertTimeConfigModal.DefaultMessage;
        }

        // Validate time format (HH:mm)
        if (!TimeFormatRegex.IsMatch(timeInput))
        {
            LogInvalidInput(_logger, Context.User.Id, $"Invalid time format: {timeInput}");
            await RespondAsync(
                new GeneralErrorComponent("Invalid time format. Please use HH:mm format (e.g., 06:00 or 14:30)."),
                new DailyAlertTimeConfigComponent());
            return;
        }

        try
        {
            // Save time
            var timePersisted = await _settingsService.SetSettingAsync(ApolloSettings.Keys.DailyAlertTime, timeInput);
            
            // Save message
            var messagePersisted = await _settingsService.SetSettingAsync(ApolloSettings.Keys.DailyAlertInitialMessage, messageInput);

            if (!timePersisted || !messagePersisted)
            {
                LogPersistenceFailed(_logger, Context.User.Id);
                await RespondAsync(
                    new GeneralErrorComponent("We couldn't save your configuration. Please try again."),
                    new DailyAlertTimeConfigComponent());
                return;
            }

            // Reload settings to update the IOptions
            await _settingsProvider.ReloadAsync();

            LogPersistenceSuccess(_logger, timeInput, Context.User.Id);

            await RespondAsync(
                new SuccessNotificationComponent(
                    "Daily alert configuration complete",
                    $"Daily updates will now be posted at **{timeInput}** with your custom message."));
        }
        catch (Exception ex)
        {
            LogPersistenceError(_logger, ex, Context.User.Id);
            await RespondAsync(
                new GeneralErrorComponent("We couldn't save your configuration. Please try again."),
                new DailyAlertTimeConfigComponent());
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
}
