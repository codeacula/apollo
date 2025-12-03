using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Users.Events;
using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Domain.Users.Models;

public sealed class User
{
  private readonly List<object> _uncommittedEvents = [];

  public UserId Id { get; private set; }
  public Username Username { get; private set; }
  public DisplayName DisplayName { get; private set; }
  public bool HasAccess { get; private set; }
  public DateTime CreatedOn { get; private set; }
  public DateTime UpdatedOn { get; private set; }
  public int Version { get; private set; }

  public IReadOnlyList<object> UncommittedEvents => _uncommittedEvents.AsReadOnly();

  private User()
  {
    Id = default;
    Username = default;
    DisplayName = default;
  }

  public static User Create(UserId id, Username username, DisplayName displayName, bool hasAccess = false)
  {
    var user = new User();
    var createdEvent = new UserCreatedEvent
    {
      UserId = id,
      Username = username,
      DisplayName = displayName,
      HasAccess = hasAccess,
      CreatedOn = DateTime.UtcNow
    };

    user.Apply(createdEvent);
    user._uncommittedEvents.Add(createdEvent);

    return user;
  }

  public void UpdateDisplayName(DisplayName displayName)
  {
    if (DisplayName.Equals(displayName))
    {
      return;
    }

    var updatedEvent = new UserUpdatedEvent
    {
      UserId = Id,
      DisplayName = displayName,
      UpdatedOn = DateTime.UtcNow
    };

    Apply(updatedEvent);
    _uncommittedEvents.Add(updatedEvent);
  }

  public void GrantAccess()
  {
    if (HasAccess)
    {
      return;
    }

    var grantedEvent = new UserAccessGrantedEvent
    {
      UserId = Id,
      GrantedOn = DateTime.UtcNow
    };

    Apply(grantedEvent);
    _uncommittedEvents.Add(grantedEvent);
  }

  public void RevokeAccess()
  {
    if (!HasAccess)
    {
      return;
    }

    var revokedEvent = new UserAccessRevokedEvent
    {
      UserId = Id,
      RevokedOn = DateTime.UtcNow
    };

    Apply(revokedEvent);
    _uncommittedEvents.Add(revokedEvent);
  }

  public void ClearUncommittedEvents()
  {
    _uncommittedEvents.Clear();
  }

  private void Apply(UserCreatedEvent @event)
  {
    Id = @event.UserId;
    Username = @event.Username;
    DisplayName = @event.DisplayName;
    HasAccess = @event.HasAccess;
    CreatedOn = @event.CreatedOn;
    UpdatedOn = @event.CreatedOn;
    Version++;
  }

  private void Apply(UserUpdatedEvent @event)
  {
    DisplayName = @event.DisplayName;
    UpdatedOn = @event.UpdatedOn;
    Version++;
  }

  private void Apply(UserAccessGrantedEvent @event)
  {
    HasAccess = true;
    UpdatedOn = @event.GrantedOn;
    Version++;
  }

  private void Apply(UserAccessRevokedEvent @event)
  {
    HasAccess = false;
    UpdatedOn = @event.RevokedOn;
    Version++;
  }

  public void Apply(object @event)
  {
    switch (@event)
    {
      case UserCreatedEvent e:
        Apply(e);
        break;
      case UserUpdatedEvent e:
        Apply(e);
        break;
      case UserAccessGrantedEvent e:
        Apply(e);
        break;
      case UserAccessRevokedEvent e:
        Apply(e);
        break;
      default:
        throw new InvalidOperationException($"Unknown event type: {@event.GetType().Name}");
    }
  }
}
