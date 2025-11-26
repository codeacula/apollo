using Apollo.Domain.ValueObjects;

namespace Apollo.Domain.Models;

public sealed record Reminder(
    ReminderId Id,
    TaskId TaskId,
    ReminderTime Time);
