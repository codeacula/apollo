namespace Apollo.Application.People.Notifications;

public record PersonCreatedNotification : INotification;
public record PersonAccessGrantedNotification : INotification;
public record PersonAccessRevokedNotification : INotification;
