using Apollo.Database.Models;

using Microsoft.EntityFrameworkCore;

namespace Apollo.Database;

public interface IApolloDbContext
{
  DbSet<ApolloUser> Users { get; }

  Task MigrateAsync(CancellationToken cancellationToken = default);

  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
