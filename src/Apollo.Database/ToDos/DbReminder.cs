using Apollo.Database.ToDos.Events;
using Apollo.Domain.ToDos.Models;

using JasperFx.Events;

namespace Apollo.Database.ToDos;

public sealed record DbReminder
{
  public required Guid Id { get; init; }
  public required Guid PersonId { get; init; }
  public required string Details { get; init; }
  public required Guid QuartzJobId { get; init; }
  public required DateTime ReminderTime { get; init; }
  public DateTime? AcknowledgedOn { get; init; }
  public DateTime? SentOn { get; init; }
  public bool IsDeleted { get; init; }
  public DateTime CreatedOn { get; init; }
  public DateTime UpdatedOn { get; init; }

  public static explicit operator Reminder(DbReminder dbReminder)
  {
    return new()
    {
      Id = new(dbReminder.Id),
      PersonId = new(dbReminder.PersonId),
      Details = new(dbReminder.Details),
      QuartzJobId = new(dbReminder.QuartzJobId),
      ReminderTime = new(dbReminder.ReminderTime),
      AcknowledgedOn = dbReminder.AcknowledgedOn.HasValue ? new(dbReminder.AcknowledgedOn.Value) : null,
      CreatedOn = new(dbReminder.CreatedOn),
      UpdatedOn = new(dbReminder.UpdatedOn)
    };
  }

  public static DbReminder Create(IEvent<ReminderCreatedEvent> ev)
  {
    var eventData = ev.Data;

    return new()
    {
      Id = eventData.Id,
      PersonId = eventData.PersonId,
      Details = eventData.Details,
      QuartzJobId = eventData.QuartzJobId,
      ReminderTime = eventData.ReminderTime,
      AcknowledgedOn = null,
      SentOn = null,
      IsDeleted = false,
      CreatedOn = eventData.CreatedOn,
      UpdatedOn = eventData.CreatedOn
    };
  }

  public static DbReminder Apply(IEvent<ReminderSentEvent> ev, DbReminder reminder)
  {
    return reminder with
    {
      SentOn = ev.Data.SentOn,
      UpdatedOn = ev.Data.SentOn
    };
  }

  public static DbReminder Apply(IEvent<ReminderAcknowledgedEvent> ev, DbReminder reminder)
  {
    return reminder with
    {
      AcknowledgedOn = ev.Data.AcknowledgedOn,
      UpdatedOn = ev.Data.AcknowledgedOn
    };
  }

  public static DbReminder Apply(IEvent<ReminderDeletedEvent> ev, DbReminder reminder)
  {
    return reminder with
    {
      IsDeleted = true,
      UpdatedOn = ev.Data.DeletedOn
    };
  }
}
