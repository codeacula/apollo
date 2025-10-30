using System.Globalization;

using Apollo.Core.Configuration;
using Apollo.Core.Services;
using Apollo.Discord.Components;
using Apollo.Discord.Services;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace Apollo.Discord.Modules;

public partial class ApolloRoleMenuInteractions(
    ILogger<ApolloRoleMenuInteractions> logger,
    ISettingsService settingsService,
    ISettingsProvider settingsProvider,
    IDailyAlertSetupSessionStore sessionStore) : ComponentInteractionModule<RoleMenuInteractionContext>
{
  private readonly ILogger<ApolloRoleMenuInteractions> _logger = logger;
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

  [ComponentInteraction(ToDoRoleSelectComponent.CustomId)]
  public async Task ConfigureDailyAlertRoleAsync()
  {
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    IReadOnlyList<ulong> selectedRoles = Context.Interaction.Data.SelectedValues;

    if (selectedRoles is not { Count: > 0 })
    {
      LogNoRoleSelected(_logger, Context.User.Id);
      _ = await RespondAsync(
          new GeneralErrorComponent("Please select a notification role before continuing."),
          new ToDoRoleSelectComponent());
      return;
    }

    ulong roleId = selectedRoles[0];

    try
    {
      bool persisted = await _settingsService.SetSettingAsync(ApolloSettings.Keys.DailyAlertRoleId, roleId.ToString(CultureInfo.InvariantCulture));

      if (!persisted)
      {
        LogPersistenceFailed(_logger, roleId, Context.User.Id);
        _ = await RespondAsync(
            new GeneralErrorComponent("We couldn't save that role selection. Please try again."),
            new ToDoRoleSelectComponent());
        return;
      }

      await _settingsProvider.ReloadAsync();
    }
    catch (Exception ex)
    {
      LogPersistenceError(_logger, ex, roleId, Context.User.Id);
      _ = await RespondAsync(
          new GeneralErrorComponent("We couldn't save that role selection. Please try again."),
          new ToDoRoleSelectComponent());
      return;
    }

    LogPersistenceSuccess(_logger, roleId, Context.User.Id);

    _ = await RespondAsync(
        new SuccessNotificationComponent("Daily alert role saved", $"The <@&{roleId}> role will now be notified for daily updates."),
        new DailyAlertTimeConfigComponent());
  }

  [ComponentInteraction(DailyAlertSetupComponent.RoleSelectCustomId)]
  public async Task UpdateRoleSelectionAsync()
  {
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    ulong? guildId = Context.Guild?.Id;
    ulong userId = Context.User.Id;

    if (!guildId.HasValue)
    {
      LogNoGuildProvided(_logger, Context.User.Username);
      _ = await RespondAsync(new GeneralErrorComponent("No guild provided."));
      return;
    }

    IReadOnlyList<ulong> selectedRoles = Context.Interaction.Data.SelectedValues;

    if (selectedRoles is not { Count: > 0 })
    {
      LogNoRoleSelected(_logger, userId);
      DailyAlertSetupSession session = await _sessionStore.GetSessionAsync(guildId.Value, userId) ?? new DailyAlertSetupSession();
      _ = await RespondAsync(
          new GeneralErrorComponent("Please select a notification role."),
          new DailyAlertSetupComponent(session.ChannelId, session.RoleId, session.Time, session.Message));
      return;
    }

    ulong roleId = selectedRoles[0];

    try
    {
      DailyAlertSetupSession session = await _sessionStore.GetSessionAsync(guildId.Value, userId) ?? new DailyAlertSetupSession();
      session.RoleId = roleId;
      await _sessionStore.SetSessionAsync(guildId.Value, userId, session);

      LogRoleStaged(_logger, roleId, userId);

      _ = await RespondAsync(new DailyAlertSetupComponent(
          session.ChannelId,
          session.RoleId,
          session.Time,
          session.Message));
    }
    catch (Exception ex)
    {
      LogSessionUpdateError(_logger, ex, userId);
      _ = await RespondAsync(new GeneralErrorComponent("We couldn't update your selection. Please try again."));
    }
  }

  [LoggerMessage(Level = LogLevel.Warning, Message = "No role selected for daily alert configuration by {UserId}")]
  private static partial void LogNoRoleSelected(ILogger logger, ulong userId);

  [LoggerMessage(Level = LogLevel.Error, Message = "Failed to persist daily alert role {RoleId} selected by {UserId}")]
  private static partial void LogPersistenceFailed(ILogger logger, ulong roleId, ulong userId);

  [LoggerMessage(Level = LogLevel.Error, Message = "Error saving daily alert role {RoleId} for user {UserId}")]
  private static partial void LogPersistenceError(ILogger logger, Exception exception, ulong roleId, ulong userId);

  [LoggerMessage(Level = LogLevel.Information, Message = "Daily alert role set to {RoleId} by {UserId}")]
  private static partial void LogPersistenceSuccess(ILogger logger, ulong roleId, ulong userId);

  [LoggerMessage(Level = LogLevel.Information, Message = "Role {RoleId} staged for user {UserId}")]
  private static partial void LogRoleStaged(ILogger logger, ulong roleId, ulong userId);

  [LoggerMessage(Level = LogLevel.Error, Message = "Error updating session for user {UserId}")]
  private static partial void LogSessionUpdateError(ILogger logger, Exception exception, ulong userId);

  [LoggerMessage(Level = LogLevel.Error, Message = "No guild provided for {GuildName}")]
  private static partial void LogNoGuildProvided(ILogger logger, string guildName);
}
