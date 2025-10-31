using Apollo.Database.Models;

using Microsoft.EntityFrameworkCore;

namespace Apollo.Database;

public class ApolloDbContext(DbContextOptions<ApolloDbContext> options) : DbContext(options), IApolloDbContext
{
  public DbSet<ApolloChat> Chats { get; set; }
  public DbSet<ApolloUser> Users { get; set; }

  public async Task MigrateAsync(CancellationToken cancellationToken = default)
  {
    await Database.MigrateAsync(cancellationToken);
  }
}
