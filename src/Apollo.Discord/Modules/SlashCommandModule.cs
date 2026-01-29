using Apollo.Core.ToDos.Requests;
using Apollo.Discord.Components;
using Apollo.Domain.People.ValueObjects;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using ApolloPlatform = Apollo.Domain.Common.Enums.Platform;

namespace Apollo.Discord.Modules;

public class SlashCommandModule(IApolloServiceClient apolloServiceClient) : ApplicationCommandModule<ApplicationCommandContext>
{
  [SlashCommand("config", "Allows you to configure your Apollo settings.")]
  public async Task ConfigAsync()
  {
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    // Fetch user & settings

    // Display settings to the user
    var container = new ComponentContainerProperties
    {
      AccentColor = new Color(0x3B5BA5),

      Components = [
        new TextDisplayProperties("### Your current settings will be displayed here."),
      ]
    };

    _ = await ModifyResponseAsync(message =>
    {
      message.Components = [container];
      message.Flags = MessageFlags.IsComponentsV2;
    });
  }

  [SlashCommand("todo", "Quickly create a new To Do")]
  public async Task CreateFastToDoAsync(
    [SlashCommandParameter(Name = "title", Description = "The title of the To Do")] string todoTitle,
    [SlashCommandParameter(Name = "description", Description = "Any notes or details about the To Do")] string todoDescription
  )
  {
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    var platformId = new PlatformId(
      Context.User.Username,
      Context.User.Id.ToString(CultureInfo.InvariantCulture),
      ApolloPlatform.Discord
    );

    var createRequest = new CreateToDoRequest
    {
      PlatformId = platformId,
      Title = todoTitle,
      Description = todoDescription,
      ReminderDate = null,
    };

    var result = await apolloServiceClient.CreateToDoAsync(createRequest, CancellationToken.None);

    if (result.IsFailed)
    {
      _ = await ModifyResponseAsync(message =>
      {
        message.Content = $"⚠️ Unable to create your to-do: {result.GetErrorMessages(", ")}";
        message.Components = [];
      });

      return;
    }

    var container = new ToDoQuickCreateComponent(result.Value, createRequest.ReminderDate);

    _ = await ModifyResponseAsync(message =>
    {
      message.Components = [container];
      message.Content = string.Empty;
      message.Flags = MessageFlags.IsComponentsV2;
    });
  }

  [SlashCommand("todos", "List your current To Dos")]
  public async Task ListToDosAsync(
    [SlashCommandParameter(Name = "include-completed", Description = "Include completed to-dos in the list")] bool includeCompleted = false)
  {
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    var platformId = new PlatformId(Context.User.Username, Context.User.Id.ToString(CultureInfo.InvariantCulture), ApolloPlatform.Discord);
    var result = await apolloServiceClient.GetToDosAsync(platformId, includeCompleted, CancellationToken.None);

    if (result.IsFailed)
    {
      _ = await ModifyResponseAsync(message =>
      {
        message.Content = $"⚠️ Unable to fetch your to-dos: {result.GetErrorMessages(", ")}";
        message.Components = [];
      });
      return;
    }

    var todos = result.Value;
    var container = new ToDoListComponent(todos, includeCompleted);

    _ = await ModifyResponseAsync(message =>
    {
      message.Components = [container];
      message.Content = string.Empty;
      message.Flags = MessageFlags.IsComponentsV2;
    });
  }

  [SlashCommand("daily_todos", "Get Apollo's suggested task list for today")]
  public async Task DailyTodosAsync()
  {
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    var platformId = new PlatformId(Context.User.Username, Context.User.Id.ToString(CultureInfo.InvariantCulture), ApolloPlatform.Discord);
    var result = await apolloServiceClient.GetDailyPlanAsync(platformId, CancellationToken.None);

    if (result.IsFailed)
    {
      _ = await ModifyResponseAsync(message =>
      {
        message.Content = $"⚠️ Unable to generate your daily plan: {result.GetErrorMessages(", ")}";
        message.Components = [];
      });
      return;
    }

    var dailyPlan = result.Value;
    var container = new DailyPlanComponent(dailyPlan);

    _ = await ModifyResponseAsync(message =>
    {
      message.Components = [container];
      message.Content = string.Empty;
      message.Flags = MessageFlags.IsComponentsV2;
    });
  }
}
