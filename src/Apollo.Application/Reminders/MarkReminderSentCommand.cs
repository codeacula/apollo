using Apollo.Application.ToDos.Notifications;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.Reminders;

public sealed record MarkReminderSentCommand(ReminderId ReminderId) : IRequest<Result>;

public sealed class MarkReminderSentCommandHandler(
  IReminderStore reminderStore,
  IMediator mediator) : IRequestHandler<MarkReminderSentCommand, Result>
{
  public async Task<Result> Handle(MarkReminderSentCommand request, CancellationToken cancellationToken)
  {
    var result = await reminderStore.MarkAsSentAsync(request.ReminderId, cancellationToken);
    if (result.IsSuccess)
    {
      await mediator.Publish(new ReminderSentNotification(), cancellationToken);
    }

    return result;
  }
}
