using Apollo.Domain.Conversations.Models;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Core.Conversations;

public interface IConversationStore
{
  Task<Conversation> GetConversationAsync(ConversationId conversationId);
  Task<Conversation> GetConversationByPersonIdAsync(PersonId personId);

  Task AddMessageAsync(ConversationId conversationId, NewMessage message);
}
