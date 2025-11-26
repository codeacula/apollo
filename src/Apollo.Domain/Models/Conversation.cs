using Apollo.Domain.ValueObjects;

namespace Apollo.Domain.Models;

public record Conversation(ConversationId Id, UserId UserId, ICollection<Message> Messages, CreatedOn CreatedOn);
