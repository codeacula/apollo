using System.Globalization;

using Apollo.Core.Configuration;
using Apollo.Core.Services;
using Apollo.Discord.Components;
using Apollo.Discord.Services;

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
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    IReadOnlyList<ulong> selectedChannels = Context.Interaction.Data.SelectedValues;

    if (selectedChannels is not { Count: > 0 })
    {
      LogNoChannelSelected(_logger, Context.User.Id);
      _ = await RespondAsync(
          new GeneralErrorComponent("Please select a forum channel before continuing."),
          new ToDoChannelSelectComponent());
      return;
    }

    ulong channelId = selectedChannels[0];

    try
    {
      bool persisted = await _settingsService.SetSettingAsync(ApolloSettings.Keys.DailyAlertChannelId, channelId.ToString(CultureInfo.InvariantCulture));

      if (!persisted)
      {
        LogPersistenceFailed(_logger, channelId, Context.User.Id);
        _ = await RespondAsync(
            new GeneralErrorComponent("We couldn't save that forum selection. Please try again."),
            new ToDoChannelSelectComponent());
        return;
      }

      await _settingsProvider.ReloadAsync();
    }
    catch (Exception ex)
    {
      LogPersistenceError(_logger, ex, channelId, Context.User.Id);
      _ = await RespondAsync(
          new GeneralErrorComponent("We couldn't save that forum selection. Please try again."),
          new ToDoChannelSelectComponent());
      return;
    }

    LogPersistenceSuccess(_logger, channelId, Context.User.Id);

    _ = await RespondAsync(
        new SuccessNotificationComponent("Daily alert forum saved", $"Daily updates will now post in <#{channelId}>."),
        new ToDoRoleSelectComponent());
  }

  [ComponentInteraction(DailyAlertSetupComponent.ChannelSelectCustomId)]
  public async Task UpdateChannelSelectionAsync()
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

    IReadOnlyList<ulong> selectedChannels = Context.Interaction.Data.SelectedValues;

    if (selectedChannels is not { Count: > 0 })
    {
      LogNoChannelSelected(_logger, userId);
      DailyAlertSetupSession session = await _sessionStore.GetSessionAsync(guildId.Value, userId) ?? new DailyAlertSetupSession();
      _ = await RespondAsync(
          new GeneralErrorComponent("Please select a forum channel."),
          new DailyAlertSetupComponent(session.ChannelId, session.RoleId, session.Time, session.Message));
      return;
    }

    ulong channelId = selectedChannels[0];

    try
    {
      DailyAlertSetupSession session = await _sessionStore.GetSessionAsync(guildId.Value, userId) ?? new DailyAlertSetupSession();
      session.ChannelId = channelId;
      await _sessionStore.SetSessionAsync(guildId.Value, userId, session);

      LogChannelStaged(_logger, channelId, userId);

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
