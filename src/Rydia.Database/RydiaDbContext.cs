namespace Rydia.Database;

using Microsoft.EntityFrameworkCore;

public class RydiaDbContext(DbContextOptions<RydiaDbContext> options) : DbContext(options)
{
}