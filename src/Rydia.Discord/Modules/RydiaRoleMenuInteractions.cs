using System.Globalization;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Rydia.Core.Configuration;
using Rydia.Core.Services;
using Rydia.Discord.Components;

namespace Rydia.Discord.Modules;

public partial class RydiaRoleMenuInteractions(
    ILogger<RydiaRoleMenuInteractions> logger,
    ISettingsService settingsService,
    ISettingsProvider settingsProvider) : ComponentInteractionModule<RoleMenuInteractionContext>
{
    private readonly ILogger<RydiaRoleMenuInteractions> _logger = logger;
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

    [ComponentInteraction(ToDoRoleSelectComponent.CustomId)]
    public async Task ConfigureDailyAlertRoleAsync()
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var selectedRoles = Context.Interaction.Data.SelectedValues;

        if (selectedRoles is not { Count: > 0 })
        {
            LogNoRoleSelected(_logger, Context.User.Id);
            await RespondAsync(
                new GeneralErrorComponent("Please select a notification role before continuing."),
                new ToDoRoleSelectComponent());
            return;
        }

        var roleId = selectedRoles[0];

        try
        {
            var persisted = await _settingsService.SetSettingAsync(RydiaSettings.Keys.DailyAlertRoleId, roleId.ToString(CultureInfo.InvariantCulture));

            if (!persisted)
            {
                LogPersistenceFailed(_logger, roleId, Context.User.Id);
                await RespondAsync(
                    new GeneralErrorComponent("We couldn't save that role selection. Please try again."),
                    new ToDoRoleSelectComponent());
                return;
            }

            // Reload settings to update the IOptions
            await _settingsProvider.ReloadAsync();
        }
        catch (Exception ex)
        {
            LogPersistenceError(_logger, ex, roleId, Context.User.Id);
            await RespondAsync(
                new GeneralErrorComponent("We couldn't save that role selection. Please try again."),
                new ToDoRoleSelectComponent());
            return;
        }

        LogPersistenceSuccess(_logger, roleId, Context.User.Id);

        await RespondAsync(
            new SuccessNotificationComponent("Daily alert role saved", $"The <@&{roleId}> role will now be notified for daily updates."));
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "No role selected for daily alert configuration by {UserId}")]
    private static partial void LogNoRoleSelected(ILogger logger, ulong userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to persist daily alert role {RoleId} selected by {UserId}")]
    private static partial void LogPersistenceFailed(ILogger logger, ulong roleId, ulong userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error saving daily alert role {RoleId} for user {UserId}")]
    private static partial void LogPersistenceError(ILogger logger, Exception exception, ulong roleId, ulong userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Daily alert role set to {RoleId} by {UserId}")]
    private static partial void LogPersistenceSuccess(ILogger logger, ulong roleId, ulong userId);
}
