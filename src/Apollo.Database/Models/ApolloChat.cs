using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apollo.Database.Models;

public class ApolloChat
{
  [Key]
  public required Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  public required Guid UserId { get; set; }

  public required string ChatText { get; set; }

  public required bool Outgoing { get; set; }

  public DateTime CreatedWhen { get; set; } = DateTime.UtcNow;

  [ForeignKey("UserId")]
  public virtual ApolloUser? User { get; set; }
}
