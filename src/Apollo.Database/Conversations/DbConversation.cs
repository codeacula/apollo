using Apollo.Database.Conversations.Events;
using Apollo.Domain.Conversations.Models;

using JasperFx.Events;

namespace Apollo.Database.Conversations;

public sealed record DbConversation
{
  public required Guid Id { get; init; }
  public required Guid PersonId { get; init; }
  public required List<DbMessage> Messages { get; init; }
  public DateTime CreatedOn { get; init; }
  public DateTime UpdatedOn { get; init; }

  public static explicit operator Conversation(DbConversation conversation)
  {
    return new()
    {
      Id = new(conversation.Id),
      PersonId = new(conversation.PersonId),
      CreatedOn = new(conversation.CreatedOn),
      UpdatedOn = new(conversation.UpdatedOn),
      Messages = conversation.Messages.ConvertAll(m => (Message)m)
    };
  }

  public static DbConversation Create(IEvent<ConversationStartedEvent> ev)
  {
    var eventData = ev.Data;

    return new()
    {
      Id = eventData.Id,
      PersonId = eventData.PersonId,
      CreatedOn = eventData.CreatedOn,
      UpdatedOn = eventData.CreatedOn,
      Messages = [],
    };
  }

  public static DbConversation Apply(IEvent<UserSentMessageEvent> ev, DbConversation conversation)
  {
    return conversation with
    {
      Messages = [.. conversation.Messages, DbMessage.Create(ev)],
      UpdatedOn = ev.Data.CreatedOn
    };
  }

  public static DbConversation Apply(IEvent<ApolloRepliedEvent> ev, DbConversation conversation)
  {
    return conversation with
    {
      Messages = [.. conversation.Messages, DbMessage.Create(ev)],
      UpdatedOn = ev.Data.CreatedOn
    };
  }
}
