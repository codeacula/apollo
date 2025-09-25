namespace Rydia.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Factory for creating RydiaDbContext instances at design time (for migrations)
/// </summary>
public class RydiaDbContextFactory : IDesignTimeDbContextFactory<RydiaDbContext>
{
    public RydiaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RydiaDbContext>();
        
        // Use a default connection string for design-time operations
        // This won't be used in production - just for generating migrations
        optionsBuilder.UseNpgsql("Host=localhost;Database=rydia_design;Username=postgres;Password=postgres");
        
        return new RydiaDbContext(optionsBuilder.Options);
    }
}