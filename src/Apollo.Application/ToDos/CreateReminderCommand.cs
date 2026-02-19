using Apollo.Core;
using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record CreateReminderCommand(
  PersonId PersonId,
  string Details,
  DateTime ReminderDate
) : IRequest<Result<Reminder>>;

public sealed class CreateReminderCommandHandler(
  IReminderStore reminderStore,
  IToDoReminderScheduler toDoReminderScheduler) : IRequestHandler<CreateReminderCommand, Result<Reminder>>
{
  public async Task<Result<Reminder>> Handle(CreateReminderCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var jobResult = await ScheduleJobAsync(request.ReminderDate, cancellationToken);
      if (jobResult.IsFailed)
      {
        return jobResult.ToResult<Reminder>();
      }

      var createResult = await CreateReminderEntityAsync(
        request.PersonId,
        request.Details,
        request.ReminderDate,
        jobResult.Value,
        cancellationToken);

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

  private async Task<Result<QuartzJobId>> ScheduleJobAsync(
    DateTime reminderDate,
    CancellationToken cancellationToken)
  {
    var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminderDate, cancellationToken);
    return jobResult.IsFailed ? Result.Fail<QuartzJobId>($"Failed to schedule reminder job: {jobResult.GetErrorMessages()}") : jobResult;
  }

  private async Task<Result<Reminder>> CreateReminderEntityAsync(
    PersonId personId,
    string details,
    DateTime reminderDate,
    QuartzJobId jobId,
    CancellationToken cancellationToken)
  {
    var reminderId = new ReminderId(Guid.NewGuid());
    var reminderDetails = new Details(details);
    var reminderTime = new ReminderTime(reminderDate);

    var createReminderResult = await reminderStore.CreateAsync(
      reminderId,
      personId,
      reminderDetails,
      reminderTime,
      jobId,
      cancellationToken);

    return createReminderResult.IsFailed
      ? Result.Fail<Reminder>($"Failed to create reminder: {createReminderResult.GetErrorMessages()}")
      : createReminderResult;
  }

  private async Task<Result> EnsureJobExistsAsync(
    DateTime reminderDate,
    CancellationToken cancellationToken)
  {
    var ensureJobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminderDate, cancellationToken);
    return ensureJobResult.IsFailed
      ? Result.Fail($"Failed to ensure job exists: {ensureJobResult.GetErrorMessages()}")
      : Result.Ok();
  }
}
