using Microsoft.EntityFrameworkCore;

namespace Apollo.Database;

public sealed class ApolloDbContext(DbContextOptions<ApolloDbContext> options) : DbContext(options), IApolloDbContext
{
  public async Task MigrateAsync(CancellationToken cancellationToken = default)
  {
    await Database.MigrateAsync(cancellationToken);
  }
}
