using Apollo.Database.Models;

using Microsoft.EntityFrameworkCore;

namespace Apollo.Database.Repository;

public interface IApolloUserRepo
{
  Task<ApolloUser> GetOrCreateApolloUserAsync(string username);
  Task<IEnumerable<ApolloChat>> GetUserChatsAsync(Guid userId);
}

public sealed class ApolloUserRepo(IApolloDbContext dbContext) : IApolloUserRepo
{
  private readonly IApolloDbContext _dbContext = dbContext;

  public async Task<ApolloUser> GetOrCreateApolloUserAsync(string username)
  {
    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

    if (user != null)
    {
      return user;
    }

    // If not, create a new user
    user = new ApolloUser { Username = username };
    _ = _dbContext.Users.Add(user);
    _ = await _dbContext.SaveChangesAsync();
    return user;
  }

  public async Task<IEnumerable<ApolloChat>> GetUserChatsAsync(Guid userId)
  {
    return await _dbContext.Chats
      .Where(chat => chat.UserId == userId)
      .OrderBy(chat => chat.CreatedWhen)
      .ToListAsync();
  }
}
