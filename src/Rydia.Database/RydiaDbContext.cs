using Microsoft.EntityFrameworkCore;
using Rydia.Database.Models;

namespace Rydia.Database;

public class RydiaDbContext(DbContextOptions<RydiaDbContext> options) : DbContext(options)
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