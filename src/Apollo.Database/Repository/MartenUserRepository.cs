using Apollo.Domain.Users.Models;
using Apollo.Domain.Users.ValueObjects;

using Marten;

namespace Apollo.Database.Repository;

public sealed class MartenUserRepository(IDocumentStore documentStore) : IUserRepository
{
  private readonly IDocumentStore _documentStore = documentStore;

  public async Task<User?> GetAsync(UserId id, CancellationToken cancellationToken = default)
  {
    await using var session = _documentStore.LightweightSession();

    // Load the event stream for the user
    var events = await session.Events.FetchStreamAsync(id.Value, token: cancellationToken);

    if (events == null || events.Count == 0)
    {
      return null;
    }

    // Rehydrate the aggregate from events
    var user = User.Create(id, default, default);

    // Clear the creation event since we'll be replaying from the stream
    user.ClearUncommittedEvents();

    foreach (var @event in events)
    {
      user.Apply(@event.Data);
    }

    return user;
  }

  public async Task SaveAsync(User user, CancellationToken cancellationToken = default)
  {
    if (user.UncommittedEvents.Count == 0)
    {
      return;
    }

    await using var session = _documentStore.LightweightSession();

    // Append uncommitted events to the stream
    _ = session.Events.Append(user.Id.Value, [.. user.UncommittedEvents]);

    await session.SaveChangesAsync(cancellationToken);

    user.ClearUncommittedEvents();
  }
}
