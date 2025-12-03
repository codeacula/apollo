namespace Apollo.Database.Repository;

/// <summary>
/// Read model for User queries. Updated by Marten projections when user events occur.
/// </summary>
public sealed class UserReadModel
{
  public Guid Id { get; set; }
  public string Username { get; set; } = string.Empty;
  public string DisplayName { get; set; } = string.Empty;
  public bool HasAccess { get; set; }
  public DateTime CreatedOn { get; set; }
  public DateTime UpdatedOn { get; set; }
}
