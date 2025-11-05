using Apollo.Database.Models;

using Microsoft.EntityFrameworkCore;

namespace Apollo.Database;

public class ApolloDbContext(DbContextOptions<ApolloDbContext> options) : DbContext(options), IApolloDbContext
{
  public DbSet<ApolloChat> Chats { get; set; }
  public DbSet<ApolloUser> Users { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    _ = modelBuilder.Entity<ApolloUser>()
      .HasIndex(u => u.Username)
      .IsUnique();
  }

  public async Task MigrateAsync(CancellationToken cancellationToken = default)
  {
    await Database.MigrateAsync(cancellationToken);
  }
}
