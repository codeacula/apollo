using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Domain.Conversations.Models;

public record Message(MessageId Id, UserId SenderId, Content Content, CreatedOn CreatedOn);
