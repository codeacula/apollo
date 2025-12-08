using Apollo.Database.Conversations.Events;
using Apollo.Domain.Conversations.Models;

using JasperFx.Events;

namespace Apollo.Database.Conversations;

public sealed record DbMessage
{
  public required Guid Id { get; init; }
  public required Guid ConversationId { get; init; }
  public required string Content { get; init; }
  public required bool FromUser { get; init; }
  public DateTime CreatedOn { get; init; }

  public static explicit operator Message(DbMessage message)
  {
    return new()
    {
      Id = new(message.Id),
      ConversationId = new(message.ConversationId),
      Content = new(message.Content),
      CreatedOn = new(message.CreatedOn),
      FromUser = new(message.FromUser)
    };
  }

  public static DbMessage Create(IEvent<ConversationStartedEvent> ev)
  {
    var eventData = ev.Data;

    return new()
    {
      Id = Guid.NewGuid(),
      ConversationId = eventData.Id,
      Content = eventData.Message,
      FromUser = true,
      CreatedOn = eventData.CreatedOn
    };
  }

  public static DbMessage Create(IEvent<UserSentMessageEvent> ev)
  {
    var eventData = ev.Data;

    return new()
    {
      Id = Guid.NewGuid(),
      ConversationId = eventData.Id,
      Content = eventData.Message,
      FromUser = true,
      CreatedOn = eventData.CreatedOn
    };
  }
}
