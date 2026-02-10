using Apollo.Core.Conversations;
using Apollo.Core.Logging;
using Apollo.Core.People;
using Apollo.Core.Reminders.Requests;
using Apollo.Core.ToDos.Requests;
using Apollo.Discord.Components;
using Apollo.Discord.Extensions;

using FluentResults;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Apollo.Discord.Handlers;

public sealed class IncomingMessageHandler(
  IApolloServiceClient apolloServiceClient,
  IPersonCache personCache,
  ILogger<IncomingMessageHandler> logger) : IMessageCreateGatewayHandler
{
  private async Task<bool> AccessIsDeniedAsync(Result<bool?> validationResult, Message arg)
  {
    if (validationResult.IsSuccess && validationResult.Value is false)
    {
      ValidationLogs.ValidationFailed(logger, arg.GetDiscordPlatformId().PlatformUserId, "Access denied");
      _ = await arg.SendAsync("Sorry, you do not have access to Apollo.");
      return true;
    }

    return false;
  }

  public async ValueTask HandleAsync(Message arg)
  {
    if (!IsDirectMessage(arg))
    {
      return;
    }

    var validationResult = await personCache.GetAccessAsync(arg.GetDiscordPlatformId());

    if (
      await ValidationFailedAsync(arg, validationResult)
      || await AccessIsDeniedAsync(validationResult, arg)
    )
    {
      return;
    }

    var content = arg.Content;

    if (QuickCommandParser.IsToDoCommand(content))
    {
      await HandleToDoCommandAsync(arg, content);
      return;
    }

    if (QuickCommandParser.IsReminderCommand(content))
    {
      await HandleReminderCommandAsync(arg, content);
      return;
    }

    await SendToServiceAsync(content, arg);
  }

  private static bool IsDirectMessage(Message message)
  {
    return message.GuildId == null && !message.Author.IsBot;
  }

  private async Task SendToServiceAsync(string content, Message arg)
  {
    var platformId = arg.GetDiscordPlatformId();

    try
    {
      var processMessage = new ProcessMessageRequest
      {
        Platform = platformId.Platform,
        PlatformUserId = platformId.PlatformUserId,
        Username = platformId.Username,
        Content = content
      };

      var response = await apolloServiceClient.SendMessageAsync(processMessage, CancellationToken.None);

      if (response.IsFailed)
      {
        _ = await arg.SendAsync($"An error occurred:\n{response.GetErrorMessages("\n")}");
        return;
      }

      _ = await arg.SendAsync(response.Value);
    }
    catch (Exception ex)
    {
      DiscordLogs.MessageProcessingFailed(logger, arg.Author.Username, platformId.PlatformUserId, ex.Message, ex);
      _ = await arg.SendAsync("Sorry, an unexpected error occurred while processing your message. Please try again later.");
    }
  }

  private async Task HandleToDoCommandAsync(Message arg, string content)
  {
    if (!QuickCommandParser.TryParseToDo(content, out var description))
    {
      _ = await arg.SendAsync("To create a todo, use: `todo <description>`\nExample: `todo Buy groceries`");
      return;
    }

    var platformId = arg.GetDiscordPlatformId();

    try
    {
      var createRequest = new CreateToDoRequest
      {
        PlatformId = platformId,
        Title = description,
        Description = description,
        ReminderDate = null,
      };

      var result = await apolloServiceClient.CreateToDoAsync(createRequest, CancellationToken.None);

      if (result.IsFailed)
      {
        _ = await arg.SendAsync($"Unable to create your to-do: {result.GetErrorMessages(", ")}");
        return;
      }

      var container = new ToDoQuickCreateComponent(result.Value, createRequest.ReminderDate);
      _ = await arg.SendAsync(new MessageProperties
      {
        Components = [container],
        Flags = MessageFlags.IsComponentsV2
      });
    }
    catch (Exception ex)
    {
      DiscordLogs.MessageProcessingFailed(logger, arg.Author.Username, platformId.PlatformUserId, ex.Message, ex);
      _ = await arg.SendAsync("Sorry, an unexpected error occurred while creating your to-do.");
    }
  }

  private async Task HandleReminderCommandAsync(Message arg, string content)
  {
    if (!QuickCommandParser.TryParseReminder(content, out var message, out var time))
    {
      _ = await arg.SendAsync("To set a reminder, use: `remind <message> in <time>`\nExamples:\n- `remind take a break in 30 minutes`\n- `remind check the oven in 1 hour`\n- `remind me to call mom in 2 hours`");
      return;
    }

    var platformId = arg.GetDiscordPlatformId();

    try
    {
      var createRequest = new CreateReminderRequest
      {
        PlatformId = platformId,
        Message = message,
        ReminderTime = $"in {time}",
      };

      var result = await apolloServiceClient.CreateReminderAsync(createRequest, CancellationToken.None);

      if (result.IsFailed)
      {
        _ = await arg.SendAsync($"Unable to set your reminder: {result.GetErrorMessages(", ")}");
        return;
      }

      var container = new ReminderCreatedComponent(result.Value);
      _ = await arg.SendAsync(new MessageProperties
      {
        Components = [container],
        Flags = MessageFlags.IsComponentsV2
      });
    }
    catch (Exception ex)
    {
      DiscordLogs.MessageProcessingFailed(logger, arg.Author.Username, platformId.PlatformUserId, ex.Message, ex);
      _ = await arg.SendAsync("Sorry, an unexpected error occurred while setting your reminder.");
    }
  }

  private async Task<bool> ValidationFailedAsync(Message arg, Result<bool?> validationResult)
  {
    if (validationResult.IsSuccess)
    {
      return false;
    }

    ValidationLogs.ValidationFailed(logger, arg.GetDiscordPlatformId().PlatformUserId, validationResult.GetErrorMessages());
    _ = await arg.SendAsync("Sorry, unable to verify your access at this time.");
    return true;
  }
}
