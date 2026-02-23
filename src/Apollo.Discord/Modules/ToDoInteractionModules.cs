using Apollo.Discord.Components;
using Apollo.Domain.People.ValueObjects;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

using ApolloPlatform = Apollo.Domain.Common.Enums.Platform;

namespace Apollo.Discord.Modules;

public class ToDoPriorityInteractionModule : ComponentInteractionModule<StringMenuInteractionContext>
{
  [ComponentInteraction(ToDoQuickCreateComponent.PrioritySelectCustomId)]
  public string HandlePriority() => $"Priority noted: {string.Join(", ", Context.SelectedValues)} (updates coming soon).";
}

public class ToDoEnergyInteractionModule : ComponentInteractionModule<StringMenuInteractionContext>
{
  [ComponentInteraction(ToDoQuickCreateComponent.EnergySelectCustomId)]
  public string HandleEnergy() => $"Energy level noted: {string.Join(", ", Context.SelectedValues)} (updates coming soon).";
}

public class ToDoInterestInteractionModule : ComponentInteractionModule<StringMenuInteractionContext>
{
  [ComponentInteraction(ToDoQuickCreateComponent.InterestSelectCustomId)]
  public string HandleInterest() => $"Interest noted: {string.Join(", ", Context.SelectedValues)} (updates coming soon).";
}

public class ToDoReminderInteractionModule : ComponentInteractionModule<ButtonInteractionContext>
{
  [ComponentInteraction(ToDoQuickCreateComponent.ReminderButtonCustomId)]
  public string HandleReminderButton() => "Reminder editing is coming soon. For now, reply with a time and I'll log it!";
}

public class ToDoEditInteractionModule : ComponentInteractionModule<ButtonInteractionContext>
{
  [ComponentInteraction(ToDoListComponent.EditButtonCustomId)]
  public string HandleEditButton()
  {
    return "Select which todo you'd like to edit via a select menu (coming soon!)";
  }
}

public class ToDoDeleteInteractionModule(IApolloServiceClient apolloServiceClient) : ComponentInteractionModule<ButtonInteractionContext>
{
  [ComponentInteraction(ToDoListComponent.DeleteButtonCustomId)]
  public async Task HandleDeleteButtonAsync()
  {
    _ = await RespondAsync(InteractionCallback.DeferredModifyMessage);

    var platformId = new PlatformId(
      Context.User.Username,
      Context.User.Id.ToString(CultureInfo.InvariantCulture),
      ApolloPlatform.Discord
    );

    var result = await apolloServiceClient.GetToDosAsync(platformId, false, CancellationToken.None);

    if (result.IsFailed)
    {
      _ = await ModifyResponseAsync(message =>
      {
        message.Content = $"⚠️ Unable to fetch your to-dos: {result.GetErrorMessages(", ")}";
        message.Components = [];
        message.Flags = MessageFlags.IsComponentsV2;
      });
      return;
    }

    var container = new ToDoDeleteComponent(result.Value);

    _ = await ModifyResponseAsync(message =>
    {
      message.Components = [container];
      message.Content = string.Empty;
      message.Flags = MessageFlags.IsComponentsV2;
    });
  }
}

public class ToDoDeleteSelectInteractionModule(IApolloServiceClient apolloServiceClient) : ComponentInteractionModule<StringMenuInteractionContext>
{
  [ComponentInteraction(ToDoDeleteComponent.SelectCustomId)]
  public async Task HandleDeleteSelectAsync()
  {
    _ = await RespondAsync(InteractionCallback.DeferredModifyMessage);

    var selectedValue = Context.SelectedValues.Count > 0 ? Context.SelectedValues[0] : null;
    if (selectedValue is null || !Guid.TryParse(selectedValue, out var toDoId))
    {
      _ = await ModifyResponseAsync(message =>
      {
        message.Content = "⚠️ Invalid selection. Please try again.";
        message.Components = [];
        message.Flags = MessageFlags.IsComponentsV2;
      });
      return;
    }

    var platformId = new PlatformId(
      Context.User.Username,
      Context.User.Id.ToString(CultureInfo.InvariantCulture),
      ApolloPlatform.Discord
    );

    var result = await apolloServiceClient.GetToDosAsync(platformId, false, CancellationToken.None);

    if (result.IsFailed)
    {
      _ = await ModifyResponseAsync(message =>
      {
        message.Content = $"⚠️ Unable to fetch to-do details: {result.GetErrorMessages(", ")}";
        message.Components = [];
        message.Flags = MessageFlags.IsComponentsV2;
      });
      return;
    }

    var todo = result.Value.FirstOrDefault(t => t.Id == toDoId);
    if (todo is null)
    {
      _ = await ModifyResponseAsync(message =>
      {
        message.Content = "⚠️ To-do not found. It may have already been deleted.";
        message.Components = [];
        message.Flags = MessageFlags.IsComponentsV2;
      });
      return;
    }

    var container = new ToDoDeleteConfirmComponent(toDoId, todo.Description);

    _ = await ModifyResponseAsync(message =>
    {
      message.Components = [container];
      message.Content = string.Empty;
      message.Flags = MessageFlags.IsComponentsV2;
    });
  }
}

public class ToDoDeleteConfirmInteractionModule(IApolloServiceClient apolloServiceClient) : ComponentInteractionModule<ButtonInteractionContext>
{
  [ComponentInteraction(ToDoDeleteConfirmComponent.ConfirmButtonCustomId)]
  public async Task HandleDeleteConfirmAsync(Guid toDoId)
  {
    _ = await RespondAsync(InteractionCallback.DeferredModifyMessage);

    var platformId = new PlatformId(
      Context.User.Username,
      Context.User.Id.ToString(CultureInfo.InvariantCulture),
      ApolloPlatform.Discord
    );

    var result = await apolloServiceClient.DeleteToDoAsync(platformId, toDoId, CancellationToken.None);

    if (result.IsFailed)
    {
      _ = await ModifyResponseAsync(message =>
      {
        message.Content = $"⚠️ Unable to delete to-do: {result.GetErrorMessages(", ")}";
        message.Components = [];
        message.Flags = MessageFlags.IsComponentsV2;
      });
      return;
    }

    _ = await ModifyResponseAsync(message =>
    {
      message.Components =
      [
        new ComponentContainerProperties
        {
          AccentColor = Constants.Colors.Success,
          Components = [new TextDisplayProperties("✅ To-do deleted successfully.")]
        }
      ];
      message.Content = string.Empty;
      message.Flags = MessageFlags.IsComponentsV2;
    });
  }
}

public class ToDoDeleteCancelInteractionModule : ComponentInteractionModule<ButtonInteractionContext>
{
  [ComponentInteraction(ToDoDeleteConfirmComponent.CancelButtonCustomId)]
  public async Task HandleDeleteCancelAsync()
  {
    _ = await RespondAsync(InteractionCallback.DeferredModifyMessage);

    _ = await ModifyResponseAsync(message =>
    {
      message.Components =
      [
        new ComponentContainerProperties
        {
          AccentColor = Constants.Colors.ApolloGreen,
          Components = [new TextDisplayProperties("Deletion cancelled.")]
        }
      ];
      message.Content = string.Empty;
      message.Flags = MessageFlags.IsComponentsV2;
    });
  }
}
