namespace Apollo.Application.ToDos.Notifications;

public record ToDoCreatedNotification : INotification;
public record ToDoUpdatedNotification : INotification;
public record ToDoCompletedNotification : INotification;
public record ToDoDeletedNotification : INotification;
public record ToDoPriorityUpdatedNotification : INotification;
public record ToDoEnergyUpdatedNotification : INotification;
public record ToDoInterestUpdatedNotification : INotification;

public record ReminderCreatedNotification : INotification;
public record ReminderLinkedToToDoNotification : INotification;
public record ReminderUnlinkedFromToDoNotification : INotification;
public record ReminderSentNotification : INotification;
public record ReminderAcknowledgedNotification : INotification;
public record ReminderDeletedNotification : INotification;
