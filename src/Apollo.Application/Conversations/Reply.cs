using Apollo.Domain.Common.ValueObjects;

namespace Apollo.Application.Conversations;

public sealed record Reply
{
  public required Content Content { get; init; }
  public required CreatedOn CreatedOn { get; init; }
  public required UpdatedOn UpdatedOn { get; init; }
}
