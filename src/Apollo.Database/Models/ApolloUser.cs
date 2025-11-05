using System.ComponentModel.DataAnnotations;

namespace Apollo.Database.Models;

public class ApolloUser
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  public string DisplayName { get; set; } = string.Empty;

  public bool HasAccess { get; set; }

  [Required]
  public required string Username { get; set; }

  public DateTime CreatedWhen { get; set; } = DateTime.UtcNow;
}
