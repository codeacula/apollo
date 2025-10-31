using Apollo.Database.Models;

using Microsoft.EntityFrameworkCore;

namespace Apollo.Database;

public class ApolloDbContext(DbContextOptions<ApolloDbContext> options) : DbContext(options), IApolloDbContext
{
  /// <summary>
  /// Database set for configuration settings
  /// </summary>
  public DbSet<Setting> Settings { get; set; }

  public async Task MigrateAsync(CancellationToken cancellationToken = default)
  {
    await Database.MigrateAsync(cancellationToken);
  }
}
