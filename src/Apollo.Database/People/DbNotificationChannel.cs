using Apollo.Domain.Common.Enums;

namespace Apollo.Database.People;

/// <summary>
/// Represents a notification channel for a person in the database.
/// </summary>
public sealed record DbNotificationChannel
{
  public Guid PersonId { get; init; }
  public NotificationChannelType Type { get; init; }
  public required string Identifier { get; init; }
  public bool IsEnabled { get; init; }
  public DateTime CreatedOn { get; init; }
  public DateTime UpdatedOn { get; init; }
}
