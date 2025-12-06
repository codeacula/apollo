using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Conversations.Models;

public record Message(MessageId Id, PersonId SenderId, Content Content, CreatedOn CreatedOn);
