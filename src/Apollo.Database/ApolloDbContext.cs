using Apollo.Database.Models;

using Microsoft.EntityFrameworkCore;

namespace Apollo.Database;

public class ApolloDbContext(DbContextOptions<ApolloDbContext> options) : DbContext(options)
{
  /// <summary>
  /// Database set for configuration settings
  /// </summary>
  public DbSet<Setting> Settings { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Configure Setting entity
    _ = modelBuilder.Entity<Setting>(entity =>
    {
      _ = entity.HasIndex(e => e.Key).IsUnique(); // Ensure key uniqueness
    });
  }
}
