using Microsoft.EntityFrameworkCore;
using Apollo.Database.Models;

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
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasIndex(e => e.Key).IsUnique(); // Ensure key uniqueness
        });
    }
}