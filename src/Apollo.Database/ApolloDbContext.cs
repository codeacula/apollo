using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;

namespace Apollo.Database;

[ExcludeFromCodeCoverage]
public class ApolloDbContext(DbContextOptions<ApolloDbContext> options) : DbContext(options), IApolloDbContext
{
  public async Task MigrateAsync(CancellationToken cancellationToken = default)
  {
    await Database.MigrateAsync(cancellationToken);
  }
}
