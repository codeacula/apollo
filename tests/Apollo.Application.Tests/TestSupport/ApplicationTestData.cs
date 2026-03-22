using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

namespace Apollo.Application.Tests.TestSupport;

internal static class ApplicationTestData
{
  public static Person CreatePerson(
    PersonId personId,
    string username = "testuser",
    string platformUserId = "123",
    Platform platform = Platform.Discord,
    string? timeZoneId = null)
  {
    PersonTimeZoneId? parsedTimeZoneId = null;
    if (timeZoneId is not null && PersonTimeZoneId.TryParse(timeZoneId, out var tzId, out _))
    {
      parsedTimeZoneId = tzId;
    }

    return new Person
    {
      Id = personId,
      PlatformId = new PlatformId(username, platformUserId, platform),
      Username = new Username(username),
      HasAccess = new HasAccess(true),
      TimeZoneId = parsedTimeZoneId,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }

  public static ToDo CreateToDo(PersonId personId, string description = "test", ToDoId? toDoId = null)
  {
    return new ToDo
    {
      Id = toDoId ?? new ToDoId(Guid.NewGuid()),
      PersonId = personId,
      Description = new Description(description),
      Priority = new Priority(Level.Green),
      Energy = new Energy(Level.Green),
      Interest = new Interest(Level.Green),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }

  public static Reminder CreateReminder(
    PersonId? personId = null,
    string details = "test",
    ReminderId? reminderId = null,
    QuartzJobId? quartzJobId = null,
    DateTime? reminderTime = null)
  {
    return new Reminder
    {
      Id = reminderId ?? new ReminderId(Guid.NewGuid()),
      PersonId = personId ?? new PersonId(Guid.NewGuid()),
      Details = new Details(details),
      ReminderTime = new ReminderTime(reminderTime ?? DateTime.UtcNow.AddMinutes(30)),
      QuartzJobId = quartzJobId,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
