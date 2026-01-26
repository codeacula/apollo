using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.Models;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Core.Conversations;

public interface IConversationStore
{
  Task<Result<Conversation>> AddMessageAsync(ConversationId conversationId, Content message, CancellationToken cancellationToken = default);
  Task<Result<Conversation>> AddReplyAsync(ConversationId conversationId, Content reply, CancellationToken cancellationToken = default);
  Task<Result<Conversation>> CreateAsync(PersonId id, CancellationToken cancellationToken = default);
  Task<Result<Conversation>> GetAsync(ConversationId conversationId, CancellationToken cancellationToken = default);
  Task<Result<Conversation>> GetConversationByPersonIdAsync(PersonId personId, CancellationToken cancellationToken = default);
  Task<Result<Conversation>> GetOrCreateConversationByPersonIdAsync(PersonId personId, CancellationToken cancellationToken = default);
}
