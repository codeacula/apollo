using Apollo.AI;
using Apollo.AI.Config;
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
  ApolloAIConfig aiConfig,
  IApolloAIAgent apolloAIAgent,
  IConversationStore conversationStore,
  IFuzzyTimeParser fuzzyTimeParser,
  ILogger<ProcessIncomingMessageCommandHandler> logger,
  IMediator mediator,
  IPersonService personService,
  IPersonCache personCache,
  IPersonStore personStore,
  PersonConfig personConfig,
  TimeProvider timeProvider
) : IRequestHandler<ProcessIncomingMessageCommand, Result<Reply>>
{
  public async Task<Result<Reply>> Handle(ProcessIncomingMessageCommand request, CancellationToken cancellationToken = default)
  {
    string? usernameForLogging = null;

    try
    {
      var validationResult = ValidateRequest(request);
      if (validationResult.IsFailed)
      {
        return validationResult;
      }

      var personId = new PersonId(request.Message.Platform, request.Message.ProviderId);
      var username = new Username(request.Message.Username);
      usernameForLogging = username.Value;

      var userResult = await GetOrCreateUserAsync(personId, username, cancellationToken);
      if (userResult.IsFailed)
      {
        return userResult.ToResult<Reply>();
      }

      var person = userResult.Value;

      var accessResult = await CheckAndCacheAccessAsync(personId, person);
      if (accessResult.IsFailed)
      {
        return accessResult;
      }

      await CaptureNotificationChannelAsync(request, username, person, cancellationToken);

      var conversationResult = await GetOrCreateConversationAsync(person, request.Message.Content, cancellationToken);
      if (conversationResult.IsFailed)
      {
        return conversationResult.ToResult<Reply>();
      }

      var response = await SendToAIAsync(conversationResult.Value, person, cancellationToken);

      await SaveReplyAsync(conversationResult.Value, response);

      return CreateReply(response);
    }
    catch (Exception ex)
    {
      DataAccessLogs.UnhandledMessageProcessingError(logger, ex, usernameForLogging ?? "unknown");
      return Result.Fail<Reply>(ex.Message);
    }
  }

  private static Result<Reply> ValidateRequest(ProcessIncomingMessageCommand request)
  {
    if (string.IsNullOrWhiteSpace(request.Message.Username))
    {
      return Result.Fail<Reply>("No username was provided.");
    }
    else if (string.IsNullOrWhiteSpace(request.Message.ProviderId))
    {
      return Result.Fail<Reply>("No provider id was provided.");
    }
    else if (string.IsNullOrWhiteSpace(request.Message.Content))
    {
      return Result.Fail<Reply>("Message content is empty.");
    }
    else
    {
      return (Result<Reply>)Result.Ok();
    }
  }

  private async Task<Result<Person>> GetOrCreateUserAsync(PersonId personId, Username username, CancellationToken cancellationToken)
  {
    var userResult = await personService.GetOrCreateAsync(personId, username, cancellationToken);

    return userResult.IsFailed
      ? Result.Fail<Person>($"Failed to get or create user {username.Value}: {userResult.GetErrorMessages()}")
      : userResult;
  }

  private async Task<Result<Reply>> CheckAndCacheAccessAsync(PersonId personId, Person person)
  {
    var hasAccess = person.HasAccess.Value;

    var cacheResult = await personCache.SetAccessAsync(personId, hasAccess);
    if (cacheResult.IsFailed)
    {
      CacheLogs.UnableToSetToCache(logger, [.. cacheResult.Errors.Select(e => e.Message)]);
    }

    return !hasAccess ? Result.Fail<Reply>($"User {personId.Value} does not have access.") : (Result<Reply>)Result.Ok();
  }

  private async Task CaptureNotificationChannelAsync(ProcessIncomingMessageCommand request, Username username, Person person, CancellationToken cancellationToken)
  {
    var channelType = request.Message.Platform switch
    {
      Platform.Discord => NotificationChannelType.Discord,
      _ => (NotificationChannelType?)null
    };

    if (!channelType.HasValue)
    {
      return;
    }

    var channel = new NotificationChannel(channelType.Value, channelType.Value.ToString(), isEnabled: true);
    var channelResult = await personStore.EnsureNotificationChannelAsync(person, channel, cancellationToken);

    if (channelResult.IsFailed)
    {
      DataAccessLogs.FailedToAddNotificationChannel(logger, username.Value, channelResult.GetErrorMessages());
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

  private async Task<string> SendToAIAsync(Conversation conversation, Person person, CancellationToken cancellationToken)
  {
    var messages = conversation.Messages
      .OrderBy(m => m.CreatedOn.Value)
      .Select(m => new ChatMessageDTO(
          m.FromUser.Value ? ChatRole.User : ChatRole.Assistant,
          m.Content.Value,
          m.CreatedOn.Value
        )
      ).ToList();

    var completionRequest = new ChatCompletionRequestDTO(aiConfig.SystemPrompt, messages);

    var toDoPlugin = new ToDoPlugin(mediator, personStore, fuzzyTimeParser, timeProvider, personConfig, person.Id);
    apolloAIAgent.AddPlugin(toDoPlugin, ToDoPlugin.PluginName);

    var personPlugin = new PersonPlugin(personStore, personConfig, person.Id);
    apolloAIAgent.AddPlugin(personPlugin, PersonPlugin.PluginName);

    return await apolloAIAgent.ChatAsync(completionRequest, cancellationToken);
  }

  private async Task SaveReplyAsync(Conversation conversation, string response)
  {
    var addReplyResult = await conversationStore.AddReplyAsync(conversation.Id, new Content(response), CancellationToken.None);

    if (addReplyResult.IsFailed)
    {
      DataAccessLogs.UnableToSaveMessageToConversation(logger, conversation.Id.Value, response);
    }
  }

  private Result<Reply> CreateReply(string response)
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
