using Apollo.Domain.ValueObjects;

namespace Apollo.Domain.Models;

public record Reminder(ReminderId Id, QuartzJobId QuartzJobId, ReminderTime ReminderTime);
