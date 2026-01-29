namespace Apollo.Database;

public interface IApolloDbContext
{
  Task MigrateAsync(CancellationToken cancellationToken = default);
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
