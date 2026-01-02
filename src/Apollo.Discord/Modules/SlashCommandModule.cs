using Apollo.Core;
using Apollo.Core.API;
using Apollo.Core.ToDos.Requests;
using Apollo.Discord.Components;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using ApolloPlatform = Apollo.Domain.Common.Enums.Platform;

namespace Apollo.Discord.Modules;

public class SlashCommandModule(IApolloAPIClient apolloAPIClient) : ApplicationCommandModule<ApplicationCommandContext>
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
  public async Task CreateFastToDoAsync([SlashCommandParameter(Name = "todo", Description = "The To Do you wish to create.")] string todo)
  {
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    var createRequest = new CreateToDoRequest
    {
      Username = Context.User.Username,
      Platform = ApolloPlatform.Discord,
      Description = todo,
      ReminderDate = null
    };

    var result = await apolloAPIClient.CreateToDoAsync(createRequest);

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
  public async Task ListToDosAsync()
  {
    _ = await RespondAsync(InteractionCallback.DeferredMessage());

    var result = await apolloAPIClient.GetToDosAsync(Context.User.Username, ApolloPlatform.Discord);

    if (result.IsFailed)
    {
      _ = await ModifyResponseAsync(message =>
      {
        message.Content = $"⚠️ Unable to fetch your to-dos: {result.GetErrorMessages(", ")}";
        message.Components = [];
      });
      return;
    }

    var todos = result.Value.ToList();

    if (todos.Count == 0)
    {
      _ = await ModifyResponseAsync(message =>
      {
        message.Content = "You have no active to-dos. 🎉";
        message.Components = [];
      });
      return;
    }

    var lines = todos.Select(t =>
    {
      var reminderText = t.ReminderDate is DateTime when
        ? $" | 🔔 <t:{new DateTimeOffset(when).ToUnixTimeSeconds()}:R>"
        : string.Empty;
      return $"• {t.Description}{reminderText}";
    });

    var content = $"You have {todos.Count} to-do(s):\n" + string.Join("\n", lines);

    _ = await ModifyResponseAsync(message =>
    {
      message.Content = content;
      message.Components = [];
    });
  }
}
