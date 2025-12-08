namespace Apollo.Database.Conversations;

public sealed record Conversation
{
  public Guid Id { get; init; }
  public required string Username { get; init; }
  public bool HasAccess { get; init; }
  public DateTime CreatedOn { get; init; }
  public DateTime UpdatedOn { get; init; }
}
