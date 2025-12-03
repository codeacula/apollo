using Apollo.Domain.Users.Models;
using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Database.Repository;

public interface IUserRepository
{
  Task<User?> GetAsync(UserId id, CancellationToken cancellationToken = default);
  Task SaveAsync(User user, CancellationToken cancellationToken = default);
}
