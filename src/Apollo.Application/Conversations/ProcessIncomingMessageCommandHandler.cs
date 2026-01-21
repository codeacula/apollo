using Apollo.AI;
using Apollo.AI.DTOs;
using Apollo.AI.Enums;
using Apollo.Application.People;
using Apollo.Application.ToDos;
using Apollo.Core;
using Apollo.Core.Conversations;
using Apollo.Core.Logging;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.Models;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Application.Conversations;

public sealed class ProcessIncomingMessageCommandHandler(
  IApolloAIAgent apolloAIAgent,
  IConversationStore conversationStore,
  IFuzzyTimeParser fuzzyTimeParser,
  ILogger<ProcessIncomingMessageCommandHandler> logger,
  IMediator mediator,
  IPersonService personService,
  IPersonStore personStore,
  PersonConfig personConfig,
  TimeProvider timeProvider
) : IRequestHandler<ProcessIncomingMessageCommand, Result<Reply>>
{
  public async Task<Result<Reply>> Handle(ProcessIncomingMessageCommand request, CancellationToken cancellationToken = default)
  {
    try
    {
      var validationResult = ValidateRequest(request);
      if (validationResult.IsFailed)
      {
        return validationResult;
      }

      var platformId = request.Message.PlatformId;

      var personResult = await personService.GetOrCreateAsync(platformId, cancellationToken);
      if (personResult.IsFailed)
      {
        return Result.Fail<Reply>($"Failed to get or create user {platformId.Username}: {personResult.GetErrorMessages()}");
      }

      var person = personResult.Value;

      if (!person.HasAccess.Value)
      {
        return Result.Fail<Reply>($"User {person.Username} does not have access.");
      }

      await CaptureNotificationChannelAsync(request, person, cancellationToken);

      var conversationResult = await GetOrCreateConversationAsync(person, request.Message.Content, cancellationToken);
      if (conversationResult.IsFailed)
      {
        return conversationResult.ToResult<Reply>();
      }

      var response = await ProcessWithAIAsync(conversationResult.Value, person, cancellationToken);

      await SaveReplyAsync(conversationResult.Value, response);

      return CreateReplyToUser(response);
    }
    catch (Exception ex)
    {
      DataAccessLogs.UnhandledMessageProcessingError(logger, ex, request.Message.PlatformId.Username ?? "unknown");
      return Result.Fail<Reply>(ex.Message);
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Looks better this way.")]
  private static Result<Reply> ValidateRequest(ProcessIncomingMessageCommand request)
  {
    if (string.IsNullOrWhiteSpace(request.Message.PlatformId.Username))
    {
      return Result.Fail<Reply>("No username was provided.");
    }

    if (string.IsNullOrWhiteSpace(request.Message.PlatformId.PlatformUserId))
    {
      return Result.Fail<Reply>("No platform id was provided.");
    }

    return string.IsNullOrWhiteSpace(request.Message.Content)
      ? Result.Fail<Reply>("Message content is empty.")
      : (Result<Reply>)Result.Ok();
  }

  private async Task CaptureNotificationChannelAsync(ProcessIncomingMessageCommand request, Person person, CancellationToken cancellationToken)
  {
    var channelType = request.Message.PlatformId.Platform switch
    {
      Platform.Discord => NotificationChannelType.Discord,
      _ => (NotificationChannelType?)null
    };

    if (!channelType.HasValue)
    {
      return;
    }

    var channel = new NotificationChannel(channelType.Value, request.Message.PlatformId.PlatformUserId, isEnabled: true);
    var channelResult = await personStore.EnsureNotificationChannelAsync(person, channel, cancellationToken);

    if (channelResult.IsFailed)
    {
      DataAccessLogs.FailedToAddNotificationChannel(logger, person.Username.Value, channelResult.GetErrorMessages());
    }
  }

  private async Task<Result<Conversation>> GetOrCreateConversationAsync(Person person, string messageContent, CancellationToken cancellationToken)
  {
    var convoResult = await conversationStore.GetOrCreateConversationByPersonIdAsync(person.Id, cancellationToken);

    if (convoResult.IsFailed)
    {
      return Result.Fail<Conversation>("Unable to fetch conversation.");
    }

    convoResult = await conversationStore.AddMessageAsync(convoResult.Value.Id, new Content(messageContent), cancellationToken);

    return convoResult.IsFailed ? Result.Fail<Conversation>("Unable to add message to conversation.") : convoResult;
  }

  private async Task<string> ProcessWithAIAsync(Conversation conversation, Person person, CancellationToken cancellationToken)
  {
    var messages = BuildMessageHistory(conversation);
    var toolMessages = BuildToolCallingMessages(conversation);
    var plugins = CreatePlugins(person);

    // Phase 1: Tool Calling
    var toolResult = await apolloAIAgent
      .CreateToolCallingRequest(toolMessages, plugins)
      .ExecuteAsync(cancellationToken);

    // Phase 2: Response Generation
    var actionsSummary = toolResult.HasToolCalls
      ? toolResult.FormatActionsSummary()
      : "None";

    var responseResult = await apolloAIAgent
      .CreateResponseRequest(messages, actionsSummary)
      .ExecuteAsync(cancellationToken);

    return !responseResult.Success
      ? $"I encountered an issue while processing your request: {responseResult.ErrorMessage}"
      : responseResult.Content;
  }

  private static List<ChatMessageDTO> BuildMessageHistory(Conversation conversation)
  {
    return [.. conversation.Messages
      .OrderBy(m => m.CreatedOn.Value)
      .Select(m => new ChatMessageDTO(
        m.FromUser.Value ? ChatRole.User : ChatRole.Assistant,
        m.Content.Value,
        m.CreatedOn.Value
      ))];
  }

  private static List<ChatMessageDTO> BuildToolCallingMessages(Conversation conversation)
  {
    var latestUserMessage = conversation.Messages
      .Where(m => m.FromUser.Value)
      .OrderByDescending(m => m.CreatedOn.Value)
      .FirstOrDefault();

    return latestUserMessage is null
      ? []
      : [
        new ChatMessageDTO(
          ChatRole.User,
          latestUserMessage.Content.Value,
          latestUserMessage.CreatedOn.Value
        )
      ];
  }

  private Dictionary<string, object> CreatePlugins(Person person)
  {
    var toDoPlugin = new ToDoPlugin(mediator, personStore, fuzzyTimeParser, timeProvider, personConfig, person.Id);
    var personPlugin = new PersonPlugin(personStore, personConfig, person.Id);

    return new Dictionary<string, object>
    {
      [ToDoPlugin.PluginName] = toDoPlugin,
      [PersonPlugin.PluginName] = personPlugin
    };
  }

  private async Task SaveReplyAsync(Conversation conversation, string response)
  {
    var addReplyResult = await conversationStore.AddReplyAsync(conversation.Id, new Content(response), CancellationToken.None);

    if (addReplyResult.IsFailed)
    {
      DataAccessLogs.UnableToSaveMessageToConversation(logger, conversation.Id.Value, response);
    }
  }

  private Result<Reply> CreateReplyToUser(string response)
  {
    var currentTime = timeProvider.GetUtcDateTime();
    return Result.Ok(new Reply
    {
      Content = new(response),
      CreatedOn = new(currentTime),
      UpdatedOn = new(currentTime)
    });
  }
}
