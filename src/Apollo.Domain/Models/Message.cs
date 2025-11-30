using Apollo.Domain.ValueObjects;

namespace Apollo.Domain.Models;

public record Message(MessageId Id, UserId SenderId, Content Content, CreatedOn CreatedOn);
