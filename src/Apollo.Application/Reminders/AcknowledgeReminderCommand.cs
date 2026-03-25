using Apollo.Application.ToDos.Notifications;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.Reminders;

public sealed record AcknowledgeReminderCommand(ReminderId ReminderId) : IRequest<Result>;

public sealed class AcknowledgeReminderCommandHandler(
  IReminderStore reminderStore,
  IMediator mediator) : IRequestHandler<AcknowledgeReminderCommand, Result>
{
  public async Task<Result> Handle(AcknowledgeReminderCommand request, CancellationToken cancellationToken)
  {
    var result = await reminderStore.AcknowledgeAsync(request.ReminderId, cancellationToken);
    if (result.IsSuccess)
    {
      await mediator.Publish(new ReminderAcknowledgedNotification(), cancellationToken);
    }

    return result;
  }
}
