using Apollo.Core;
using Apollo.Core.Conversations;
using Apollo.Database.Conversations.Events;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.Models;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

using Marten;

namespace Apollo.Database.Conversations;

public sealed class ConversationStore(IDocumentSession session, TimeProvider timeProvider) : IConversationStore
{
  public async Task<Result<Conversation>> AddMessageAsync(ConversationId conversationId, Content message, CancellationToken cancellationToken = default)
  {
    try
    {
      _ = session.Events.Append(conversationId.Value, new UserSentMessageEvent
      {
        Id = conversationId.Value,
        Message = message.Value,
        CreatedOn = timeProvider.GetUtcDateTime()
      });

      await session.SaveChangesAsync(cancellationToken);

      var conversation = await GetAsync(conversationId, cancellationToken);

      return Result.Ok(conversation.Value);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<Conversation>> AddReplyAsync(ConversationId conversationId, Content reply, CancellationToken cancellationToken = default)
  {
    try
    {
      _ = session.Events.Append(conversationId.Value, new ApolloRepliedEvent
      {
        Id = conversationId.Value,
        Message = reply.Value,
        CreatedOn = timeProvider.GetUtcDateTime()
      });

      await session.SaveChangesAsync(cancellationToken);

      var conversation = await GetAsync(conversationId, cancellationToken);

      return Result.Ok(conversation.Value);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<Conversation>> CreateAsync(PersonId id, CancellationToken cancellationToken = default)
  {
    try
    {
      var conversationId = Guid.NewGuid();
      var ev = new ConversationStartedEvent
      {
        Id = conversationId,
        PersonId = id.Value,
        CreatedOn = timeProvider.GetUtcDateTime()
      };

      _ = session.Events.StartStream<DbConversation>(conversationId, [ev]);
      await session.SaveChangesAsync(cancellationToken);

      var newConversation = await session.Events.AggregateStreamAsync<DbConversation>(conversationId, token: cancellationToken);

      return newConversation is null ? Result.Fail<Conversation>($"Failed to create new conversation for person {id.Value}") : Result.Ok((Conversation)newConversation);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<Conversation>> GetAsync(ConversationId conversationId, CancellationToken cancellationToken = default)
  {
    try
    {
      var conversation = await session.Query<DbConversation>().FirstOrDefaultAsync(u => u.Id == conversationId.Value, cancellationToken);
      return conversation is null ? Result.Fail<Conversation>("No conversation found.") : Result.Ok((Conversation)conversation);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<Conversation>> GetConversationByPersonIdAsync(PersonId personId, CancellationToken cancellationToken = default)
  {
    try
    {
      var conversation = await session.Query<DbConversation>()
        .FirstOrDefaultAsync(u => u.PersonId == personId.Value, cancellationToken);
      return conversation is null ? Result.Fail<Conversation>("No conversation found.") : Result.Ok((Conversation)conversation);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<Conversation>> GetOrCreateConversationByPersonIdAsync(PersonId personId, CancellationToken cancellationToken = default)
  {
    try
    {
      var conversation = await session.Query<DbConversation>()
        .FirstOrDefaultAsync(u => u.PersonId == personId.Value, cancellationToken);

      return conversation is not null ? Result.Ok((Conversation)conversation) : await CreateAsync(personId, cancellationToken);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
