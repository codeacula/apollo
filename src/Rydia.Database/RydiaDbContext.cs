namespace Rydia.Database;

using Microsoft.EntityFrameworkCore;
using Rydia.Database.Models;

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
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique(); // Ensure key uniqueness
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });
    }
}