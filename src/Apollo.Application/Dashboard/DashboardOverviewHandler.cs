using Apollo.Application.Configuration.Notifications;
using Apollo.Application.Conversations.Notifications;
using Apollo.Application.People.Notifications;
using Apollo.Application.ToDos.Notifications;
using Apollo.Core.Dashboard;

namespace Apollo.Application.Dashboard;

public sealed class DashboardOverviewHandler(IDashboardUpdatePublisher publisher) :
  INotificationHandler<AiConfigurationUpdatedNotification>,
  INotificationHandler<DiscordConfigurationUpdatedNotification>,
  INotificationHandler<SuperAdminConfigurationUpdatedNotification>,
  INotificationHandler<ConversationCreatedNotification>,
  INotificationHandler<MessageAddedNotification>,
  INotificationHandler<ReplyAddedNotification>,
  INotificationHandler<PersonCreatedNotification>,
  INotificationHandler<PersonAccessGrantedNotification>,
  INotificationHandler<PersonAccessRevokedNotification>,
  INotificationHandler<ToDoCreatedNotification>,
  INotificationHandler<ToDoUpdatedNotification>,
  INotificationHandler<ToDoCompletedNotification>,
  INotificationHandler<ToDoDeletedNotification>,
  INotificationHandler<ToDoPriorityUpdatedNotification>,
  INotificationHandler<ToDoEnergyUpdatedNotification>,
  INotificationHandler<ToDoInterestUpdatedNotification>,
  INotificationHandler<ReminderCreatedNotification>,
  INotificationHandler<ReminderLinkedToToDoNotification>,
  INotificationHandler<ReminderUnlinkedFromToDoNotification>,
  INotificationHandler<ReminderSentNotification>,
  INotificationHandler<ReminderAcknowledgedNotification>,
  INotificationHandler<ReminderDeletedNotification>
{
  public Task Handle(AiConfigurationUpdatedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(DiscordConfigurationUpdatedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(SuperAdminConfigurationUpdatedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ConversationCreatedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(MessageAddedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ReplyAddedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(PersonCreatedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(PersonAccessGrantedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(PersonAccessRevokedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ToDoCreatedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ToDoUpdatedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ToDoCompletedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ToDoDeletedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ToDoPriorityUpdatedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ToDoEnergyUpdatedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ToDoInterestUpdatedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ReminderCreatedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ReminderLinkedToToDoNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ReminderUnlinkedFromToDoNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ReminderSentNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ReminderAcknowledgedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);

  public Task Handle(ReminderDeletedNotification notification, CancellationToken cancellationToken) =>
    publisher.PublishOverviewUpdatedAsync(cancellationToken);
}
