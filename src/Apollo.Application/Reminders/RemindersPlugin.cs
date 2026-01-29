using System.ComponentModel;

using Apollo.Application.Reminders.Commands;
using Apollo.Application.ToDos.Commands;
using Apollo.Core;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Microsoft.SemanticKernel;

namespace Apollo.Application.Reminders;

public sealed class RemindersPlugin(
  IMediator mediator,
  IPersonStore personStore,
  IFuzzyTimeParser fuzzyTimeParser,
  TimeProvider timeProvider,
  PersonConfig personConfig,
  PersonId personId)
{
  public const string PluginName = "Reminders";

  [KernelFunction("create_reminder")]
  [Description("Creates a one-time reminder notification. Use this for ephemeral reminders like 'take a break', 'check the oven', 'stand up and stretch' - things that don't need to be tracked as tasks. For persistent tasks that should appear in a todo list, use ToDos.create_todo instead. Supports fuzzy times like 'in 10 minutes', 'in 2 hours', 'tomorrow', or ISO 8601 format.")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Maintains better code readability")]
  public async Task<string> CreateReminderAsync(
    [Description("What to remind the user about (e.g., 'take a break', 'check the oven')")] string message,
    [Description("When to send the reminder. Supports fuzzy formats like 'in 10 minutes', 'in 2 hours', 'tomorrow', 'next week', or ISO 8601 format (e.g., 2025-12-31T10:00:00).")] string reminderTime,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var parsedTime = await ParseReminderTimeAsync(reminderTime, cancellationToken);
      if (parsedTime.IsFailed)
      {
        return $"Failed to create reminder: {parsedTime.GetErrorMessages()}";
      }

      var command = new CreateReminderCommand(
        personId,
        message,
        parsedTime.Value
      );

      var result = await mediator.Send(command, cancellationToken);

      if (result.IsFailed)
      {
        return $"Failed to create reminder: {result.GetErrorMessages()}";
      }

      return $"Successfully created reminder '{message}' for {parsedTime.Value:yyyy-MM-dd HH:mm:ss} UTC.";
    }
    catch (Exception ex)
    {
      return $"Error creating reminder: {ex.Message}";
    }
  }

  [KernelFunction("cancel_reminder")]
  [Description("Cancels a pending reminder by its ID")]
  public async Task<string> CancelReminderAsync(
    [Description("The reminder ID (GUID)")] string reminderId,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var guidResult = TryParseReminderId(reminderId);
      if (guidResult.IsFailed)
      {
        return $"Failed to cancel reminder: {guidResult.GetErrorMessages()}";
      }

      var command = new CancelReminderCommand(personId, new ReminderId(guidResult.Value));
      var result = await mediator.Send(command, cancellationToken);

      return result.IsFailed
        ? $"Failed to cancel reminder: {result.GetErrorMessages()}"
        : "Successfully canceled the reminder.";
    }
    catch (Exception ex)
    {
      return $"Error canceling reminder: {ex.Message}";
    }
  }

  private static Result<Guid> TryParseReminderId(string reminderId)
  {
    return !Guid.TryParse(reminderId, out var reminderGuid)
      ? (Result<Guid>)Result.Fail("Invalid reminder ID format. The ID must be a valid GUID.")
      : Result.Ok(reminderGuid);
  }

  private async Task<Result<DateTime>> ParseReminderTimeAsync(string? reminderTime, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(reminderTime))
    {
      return Result.Fail<DateTime>("Reminder time is required.");
    }

    // First, try to parse as fuzzy time (e.g., "in 10 minutes")
    var fuzzyResult = fuzzyTimeParser.TryParseFuzzyTime(reminderTime, timeProvider.GetUtcNow().UtcDateTime);
    if (fuzzyResult.IsSuccess)
    {
      return Result.Ok(fuzzyResult.Value);
    }

    // Fall back to ISO 8601 parsing
    if (!DateTime.TryParse(reminderTime, out var parsedDate))
    {
      return Result.Fail<DateTime>("Invalid reminder time format. Use fuzzy time like 'in 10 minutes' or ISO 8601 format like 2025-12-31T10:00:00.");
    }

    return await ConvertToUtcAsync(parsedDate, cancellationToken);
  }

  private async Task<Result<DateTime>> ConvertToUtcAsync(DateTime parsedDate, CancellationToken cancellationToken)
  {
    // Get user's timezone or use default
    var personResult = await personStore.GetAsync(personId, cancellationToken);
    var timeZoneId = personResult.IsSuccess && personResult.Value.TimeZoneId.HasValue
      ? personResult.Value.TimeZoneId.Value.Value
      : personConfig.DefaultTimeZoneId;

    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

    // Convert from user's local time to UTC
    var reminder = parsedDate.Kind switch
    {
      DateTimeKind.Unspecified => TimeZoneInfo.ConvertTimeToUtc(parsedDate, timeZoneInfo),
      DateTimeKind.Local => parsedDate.ToUniversalTime(),
      _ => parsedDate
    };

    return Result.Ok(reminder);
  }
}
