using Apollo.AI.Enums;
using Apollo.Application.Conversations;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.Models;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Application.Tests.Conversations;

public class ConversationHistoryBuilderTests
{
  [Fact]
  public void BuildForToolPlanningIncludesRecentMessagesAndTimezoneContext()
  {
    var personId = new PersonId(Guid.NewGuid());
    var conversationId = new ConversationId(Guid.NewGuid());

    var messages = new List<Message>
    {
      CreateMessage(conversationId, personId, "What timezone are you in?", false, DateTime.UtcNow.AddMinutes(-10)),
      CreateMessage(conversationId, personId, "PST", true, DateTime.UtcNow.AddMinutes(-9)),
      CreateMessage(conversationId, personId, "Add buy milk", true, DateTime.UtcNow.AddMinutes(-8)),
      CreateMessage(conversationId, personId, "Added!", false, DateTime.UtcNow.AddMinutes(-7)),
      CreateMessage(conversationId, personId, "Also add eggs", true, DateTime.UtcNow.AddMinutes(-6)),
      CreateMessage(conversationId, personId, "Done", false, DateTime.UtcNow.AddMinutes(-5)),
      CreateMessage(conversationId, personId, "Remind me tomorrow", true, DateTime.UtcNow.AddMinutes(-4)),
      CreateMessage(conversationId, personId, "Sure", false, DateTime.UtcNow.AddMinutes(-3))
    };

    var conversation = new Conversation
    {
      Id = conversationId,
      PersonId = personId,
      Messages = messages,
      CreatedOn = new CreatedOn(DateTime.UtcNow.AddMinutes(-10)),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    var history = ConversationHistoryBuilder.BuildForToolPlanning(conversation, []);

    Assert.Contains(history, m => m.Content.Contains("timezone", StringComparison.OrdinalIgnoreCase));
    Assert.Contains(history, m => m.Content == "Sure" && m.Role == ChatRole.Assistant);
    Assert.True(history.Count >= 6);
  }

  private static Message CreateMessage(
    ConversationId conversationId,
    PersonId personId,
    string content,
    bool fromUser,
    DateTime createdOn)
  {
    return new Message
    {
      Id = new MessageId(Guid.NewGuid()),
      ConversationId = conversationId,
      PersonId = personId,
      Content = new Content(content),
      CreatedOn = new CreatedOn(createdOn),
      FromUser = new FromUser(fromUser)
    };
  }
}
