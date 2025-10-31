using Apollo.Database.Models;

using Microsoft.EntityFrameworkCore;

namespace Apollo.Database;

public interface IApolloDbContext
{
  DbSet<Setting> Settings { get; set; }

  Task MigrateAsync(CancellationToken cancellationToken = default);
}
