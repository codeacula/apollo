
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Apollo.Database;
/// <summary>
/// Factory for creating ApolloDbContext instances at design time (for migrations)
/// </summary>
public class ApolloDbContextFactory : IDesignTimeDbContextFactory<ApolloDbContext>
{
  public ApolloDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<ApolloDbContext>();

    // Use a default connection string for design-time operations
    // This won't be used in production - just for generating migrations
    _ = optionsBuilder.UseNpgsql("Host=localhost;Database=rydia_design;Username=postgres;Password=postgres");

    return new ApolloDbContext(optionsBuilder.Options);
  }
}
