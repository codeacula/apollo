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
      var toDoResult = await GetToDoAsync(request.ToDoId, cancellationToken);
      if (toDoResult.IsFailed)
      {
        return toDoResult.ToResult<Reminder>();
      }

      var jobResult = await ScheduleJobAsync(request.ReminderDate, cancellationToken);
      if (jobResult.IsFailed)
      {
        return jobResult.ToResult<Reminder>();
      }

      var createResult = await CreateAndLinkReminderAsync(toDoResult.Value, request.ReminderDate, jobResult.Value, cancellationToken);
      if (createResult.IsFailed)
      {
        return createResult;
      }

      var ensureResult = await EnsureJobExistsAsync(request.ReminderDate, cancellationToken);
      return ensureResult.IsFailed
        ? Result.Fail<Reminder>($"Reminder created but failed to ensure job exists: {ensureResult.GetErrorMessages()}")
        : createResult;
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  private async Task<Result<ToDo>> GetToDoAsync(ToDoId toDoId, CancellationToken cancellationToken)
  {
    return await toDoStore.GetAsync(toDoId, cancellationToken);
  }

  private async Task<Result<QuartzJobId>> ScheduleJobAsync(DateTime reminderDate, CancellationToken cancellationToken)
  {
    var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminderDate, cancellationToken);
    return jobResult.IsFailed
      ? Result.Fail<QuartzJobId>($"Failed to schedule reminder job: {jobResult.GetErrorMessages()}")
      : jobResult;
  }

  private async Task<Result<Reminder>> CreateAndLinkReminderAsync(ToDo todo, DateTime reminderDate, QuartzJobId jobId, CancellationToken cancellationToken)
  {
    var reminderId = new ReminderId(Guid.NewGuid());
    var reminderDetails = new Details(todo.Description.Value);
    var reminderTime = new ReminderTime(reminderDate);

    var createReminderResult = await reminderStore.CreateAsync(
      reminderId,
      todo.PersonId,
      reminderDetails,
      reminderTime,
      jobId,
      cancellationToken);

    if (createReminderResult.IsFailed)
    {
      return Result.Fail<Reminder>($"Failed to create reminder: {createReminderResult.GetErrorMessages()}");
    }

    var linkResult = await reminderStore.LinkToToDoAsync(reminderId, todo.Id, cancellationToken);
    return linkResult.IsFailed
      ? Result.Fail<Reminder>($"Reminder created but failed to link to ToDo: {linkResult.GetErrorMessages()}")
      : createReminderResult;
  }

  private async Task<Result> EnsureJobExistsAsync(DateTime reminderDate, CancellationToken cancellationToken)
  {
    var ensureJobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminderDate, cancellationToken);
    return ensureJobResult.IsFailed
      ? Result.Fail(ensureJobResult.GetErrorMessages())
      : Result.Ok();
  }
}
