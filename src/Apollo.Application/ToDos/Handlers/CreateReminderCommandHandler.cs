using Apollo.Application.ToDos.Commands;
using Apollo.Core;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos.Handlers;

public sealed class CreateReminderCommandHandler(
  IReminderStore reminderStore,
  IToDoReminderScheduler toDoReminderScheduler) : IRequestHandler<CreateReminderCommand, Result<Reminder>>
{
  public async Task<Result<Reminder>> Handle(CreateReminderCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(request.ReminderDate, cancellationToken);
      if (jobResult.IsFailed)
      {
        return Result.Fail<Reminder>($"Failed to schedule reminder job: {jobResult.GetErrorMessages()}");
      }

      var reminderId = new ReminderId(Guid.NewGuid());
      var reminderDetails = new Details(request.Details);
      var reminderTime = new ReminderTime(request.ReminderDate);

      var createReminderResult = await reminderStore.CreateAsync(
        reminderId,
        request.PersonId,
        reminderDetails,
        reminderTime,
        jobResult.Value,
        cancellationToken);

      if (createReminderResult.IsFailed)
      {
        return Result.Fail<Reminder>($"Failed to create reminder: {createReminderResult.GetErrorMessages()}");
      }

      var ensureJobResult = await toDoReminderScheduler.GetOrCreateJobAsync(request.ReminderDate, cancellationToken);
      return ensureJobResult.IsFailed
        ? Result.Fail<Reminder>($"Reminder created but failed to ensure job exists: {ensureJobResult.GetErrorMessages()}") : (Result<Reminder>)createReminderResult.Value;
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
