
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Apollo.Database;

/// <summary>
/// Factory for creating ApolloDbContext instances at design time (for migrations)
/// Do not delete this file, as it is referenced during migration running from the CLI
/// </summary>
public class ApolloDbContextFactory : IDesignTimeDbContextFactory<ApolloDbContext>
{
  public ApolloDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<ApolloDbContext>();

    // Use a default connection string for design-time operations
    // This won't be used in production - just for generating migrations
    _ = optionsBuilder.UseNpgsql("Host=localhost;Database=apollo_db;Username=apollo;Password=apollo");

    return new ApolloDbContext(optionsBuilder.Options);
  }
}
