using Apollo.AI;
using Apollo.AI.Config;
using Apollo.AI.DTOs;
using Apollo.AI.Enums;
using Apollo.Application.People;
using Apollo.Application.ToDos;
using Apollo.Core.Conversations;
using Apollo.Core.Logging;
using Apollo.Core.People;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Application.Conversations;

public sealed class ProcessIncomingMessageCommandHandler(
  ApolloAIConfig aiConfig,
  IApolloAIAgent apolloAIAgent,
  IConversationStore conversationStore,
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
    try
    {
      if (string.IsNullOrEmpty(request.Message.Username))
      {
        return Result.Fail<Reply>("No username was provided.");
      }

      if (string.IsNullOrWhiteSpace(request.Message.Content))
      {
        return Result.Fail<Reply>("Message content is empty.");
      }

      var username = new Username(request.Message.Username, request.Message.Platform);
      var userResult = await personService.GetOrCreateAsync(username, cancellationToken);

      if (userResult.IsFailed)
      {
        return Result.Fail<Reply>($"Failed to get or create user {request.Message.Username}: {string.Join(", ", userResult.Errors.Select(e => e.Message))}");
      }

      // Check user for access
      var hasAccess = userResult.Value.HasAccess.Value;

      var cacheResult = await personCache.SetAccessAsync(username, hasAccess);

      if (cacheResult.IsFailed)
      {
        CacheLogs.UnableToSetToCache(logger, [.. cacheResult.Errors.Select(e => e.Message)]);
      }

      if (!hasAccess)
      {
        return Result.Fail<Reply>($"User {username.Value} does not have access.");
      }

      var convoResult = await conversationStore.GetOrCreateConversationByPersonIdAsync(userResult.Value.Id, cancellationToken);

      if (convoResult.IsFailed)
      {
        return Result.Fail<Reply>("Unable to fetch conversation.");
      }

      convoResult = await conversationStore.AddMessageAsync(convoResult.Value.Id, new Content(request.Message.Content), cancellationToken);

      if (convoResult.IsFailed)
      {
        return Result.Fail<Reply>("Unable to add message to conversation.");
      }

      var conversation = convoResult.Value;

      string systemPrompt = aiConfig.SystemPrompt;

      var messages = conversation.Messages.Select(m => new ChatMessageDTO(
            m.FromUser.Value ? ChatRole.User : ChatRole.Assistant,
            m.Content.Value,
            m.CreatedOn.Value
          )
        ).ToList();

      var completionRequest = new ChatCompletionRequestDTO(systemPrompt, messages);

      // Register user-scoped plugins
      var toDoPlugin = new ToDoPlugin(mediator, personStore, personConfig, userResult.Value.Id);
      apolloAIAgent.AddPlugin(toDoPlugin, "ToDos");

      var personPlugin = new PersonPlugin(personStore, personConfig, userResult.Value.Id);
      apolloAIAgent.AddPlugin(personPlugin, "Person");

      // Hand message to AI here
      var response = await apolloAIAgent.ChatAsync(completionRequest, cancellationToken);

      var addReplyResult = await conversationStore.AddReplyAsync(conversation.Id, new Content(response), cancellationToken);

      if (addReplyResult.IsFailed)
      {
        // Log the error but still return the AI response
        DataAccessLogs.UnableToSaveMessageToConversation(logger, conversation.Id.Value, response);
      }

      var currentTime = timeProvider.GetUtcNow().DateTime;
      return Result.Ok(new Reply
      {
        Content = new(response),
        CreatedOn = new(currentTime),
        UpdatedOn = new(currentTime)
      });
    }
    catch (Exception ex)
    {
      return Result.Fail<Reply>(ex.Message);
    }
  }
}
