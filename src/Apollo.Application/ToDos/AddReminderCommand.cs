using Apollo.Core;
using Apollo.Core.ToDos;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record AddReminderCommand(
  ToDoId ToDoId,
  DateTime ReminderDate
) : IRequest<Result<Reminder>>;

public sealed class AddReminderCommandHandler(
  IToDoStore toDoStore,
  IReminderStore reminderStore,
  IToDoReminderScheduler toDoReminderScheduler) : IRequestHandler<AddReminderCommand, Result<Reminder>>
{
  public async Task<Result<Reminder>> Handle(AddReminderCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var toDoResult = await toDoStore.GetAsync(request.ToDoId, cancellationToken);
      if (toDoResult.IsFailed)
      {
        return Result.Fail<Reminder>(toDoResult.GetErrorMessages());
      }

      var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(request.ReminderDate, cancellationToken);
      if (jobResult.IsFailed)
      {
        return Result.Fail<Reminder>($"Failed to schedule reminder job: {jobResult.GetErrorMessages()}");
      }

      var reminderId = new ReminderId(Guid.NewGuid());
      var reminderDetails = new Details(toDoResult.Value.Description.Value);
      var reminderTime = new ReminderTime(request.ReminderDate);

      var createReminderResult = await reminderStore.CreateAsync(
        reminderId,
        toDoResult.Value.PersonId,
        reminderDetails,
        reminderTime,
        jobResult.Value,
        cancellationToken);

      if (createReminderResult.IsFailed)
      {
        return Result.Fail<Reminder>($"Failed to create reminder: {createReminderResult.GetErrorMessages()}");
      }

      var linkResult = await reminderStore.LinkToToDoAsync(reminderId, request.ToDoId, cancellationToken);
      if (linkResult.IsFailed)
      {
        return Result.Fail<Reminder>($"Reminder created but failed to link to ToDo: {linkResult.GetErrorMessages()}");
      }

      var ensureJobResult = await toDoReminderScheduler.GetOrCreateJobAsync(request.ReminderDate, cancellationToken);
      return ensureJobResult.IsFailed
        ? Result.Fail<Reminder>($"Reminder created but failed to ensure job exists: {ensureJobResult.GetErrorMessages()}")
        : (Result<Reminder>)createReminderResult.Value;
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
