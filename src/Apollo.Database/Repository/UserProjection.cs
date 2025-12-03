using Apollo.Domain.Users.Events;

namespace Apollo.Database.Repository;

/// <summary>
/// Marten projection that updates UserReadModel when user events occur.
/// This keeps the read model in sync with the event stream.
/// </summary>
public sealed class UserProjection
{
  public static UserReadModel Create(UserCreatedEvent @event)
  {
    return new UserReadModel
    {
      Id = @event.UserId.Value,
      Username = @event.Username.Value,
      DisplayName = @event.DisplayName.Value,
      HasAccess = @event.HasAccess,
      CreatedOn = @event.CreatedOn,
      UpdatedOn = @event.CreatedOn
    };
  }

  public static UserReadModel Apply(UserUpdatedEvent @event, UserReadModel view)
  {
    view.DisplayName = @event.DisplayName.Value;
    view.UpdatedOn = @event.UpdatedOn;
    return view;
  }

  public static UserReadModel Apply(UserAccessGrantedEvent @event, UserReadModel view)
  {
    view.HasAccess = true;
    view.UpdatedOn = @event.GrantedOn;
    return view;
  }

  public static UserReadModel Apply(UserAccessRevokedEvent @event, UserReadModel view)
  {
    view.HasAccess = false;
    view.UpdatedOn = @event.RevokedOn;
    return view;
  }
}
