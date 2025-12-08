using Apollo.AI;
using Apollo.AI.Config;
using Apollo.AI.DTOs;
using Apollo.AI.Enums;
using Apollo.Core.Conversations;
using Apollo.Core.Logging;
using Apollo.Core.People;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Application.Conversations;

public sealed class ProcessIncomingMessageCommandHandler(
  ApolloAIConfig aiConfig,
  IApolloAIAgent apolloAIAgent,
  IConversationStore conversationStore,
  ILogger<ProcessIncomingMessageCommandHandler> logger,
  IPersonService personService,
  IPersonCache personCache
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

      if (string.IsNullOrWhiteSpace(request.Message.Content))
      {
        return Result.Fail<Reply>("Message content is empty.");
      }

      var convoResult = await conversationStore.GetOrCreateConversationByPersonIdAsync(userResult.Value.Id, new(request.Message.Content), cancellationToken);

      if (convoResult.IsFailed)
      {
        return Result.Fail<Reply>("Unable to fetch conversation.");
      }

      var conversation = convoResult.Value;

      _ = await conversationStore.AddMessageAsync(conversation.Id, new Content(request.Message.Content), cancellationToken);

      conversation.Messages.Add(new Message
      {
        Id = new(Guid.NewGuid()),
        ConversationId = conversation.Id,
        PersonId = userResult.Value.Id,
        Content = new(request.Message.Content),
        CreatedOn = new(DateTime.UtcNow),
        FromUser = new(true),
      });

      string systemPrompt = aiConfig.SystemPrompt;

      var messages = conversation.Messages.Select(m => new ChatMessage(
            m.FromUser.Value ? ChatRole.User : ChatRole.Assistant,
            m.Content.Value,
            m.CreatedOn.Value
          )
        ).ToList();

      var completionRequest = new ChatCompletionRequest(systemPrompt, messages);

      // Hand message to AI here
      var response = await apolloAIAgent.ChatAsync(completionRequest, cancellationToken);

      return Result.Ok(new Reply
      {
        Content = new(response),
        CreatedOn = new(DateTime.UtcNow),
        UpdatedOn = new(DateTime.UtcNow)
      });
    }
    catch (Exception ex)
    {
      return Result.Fail<Reply>(ex.Message);
    }
  }
}
