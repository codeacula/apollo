using Apollo.Core.Services;
using Apollo.Discord.Components;
using Apollo.Discord.Services;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Apollo.Discord.Modules;

public partial class ApolloApplicationCommands(
    ILogger<ApolloApplicationCommands> logger,
    ISettingsProvider settingsProvider,
    IDailyAlertSetupSessionStore sessionStore) : ApplicationCommandModule<ApplicationCommandContext>
{
  private readonly ILogger<ApolloApplicationCommands> _logger = logger;
  private readonly ISettingsProvider _settingsProvider = settingsProvider;
  private readonly IDailyAlertSetupSessionStore _sessionStore = sessionStore;

  private Task<RestMessage> RespondAsync(IMessageComponentProperties component)
  {
    return ModifyResponseAsync(message =>
    {
      message.Components = [component];
      message.Flags = MessageFlags.IsComponentsV2;
    });
  }


  [SlashCommand("configure-daily-alert", "Set up which forum daily alerts are posted to.")]
  public async Task ConfigureDailyAlertAsync()
  {
    LogStartConfigure(_logger, Context.User.Username);
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    if (Context.Guild is null)
    {
      LogNoGuildProvided(_logger, Context.User.Username);
      _ = await RespondAsync(new GeneralErrorComponent("No guild provided."));
      return;
    }

    ulong guildId = Context.Guild.Id;
    ulong userId = Context.User.Id;

    DailyAlertSetupSession? session = await _sessionStore.GetSessionAsync(guildId, userId);

    if (session is null)
    {
      Core.Configuration.ApolloSettings settings = _settingsProvider.GetSettings();
      session = new DailyAlertSetupSession
      {
        ChannelId = settings.DailyAlertChannelId,
        RoleId = settings.DailyAlertRoleId,
        Time = settings.DailyAlertTime,
        Message = settings.DailyAlertInitialMessage
      };
      await _sessionStore.SetSessionAsync(guildId, userId, session);
    }

    _ = await RespondAsync(new DailyAlertSetupComponent(
        session.ChannelId,
        session.RoleId,
        session.Time,
        session.Message));
  }

  [LoggerMessage(
      Level = LogLevel.Information,
      Message = "configure-daily-alert initialized by {Username}"
  )]
  public static partial void LogStartConfigure(ILogger logger, string username);

  [LoggerMessage(
      Level = LogLevel.Error,
      Message = "No guild provided for {GuildName}"
  )]
  public static partial void LogNoGuildProvided(ILogger logger, string guildName);
}
