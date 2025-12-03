using Apollo.Domain.Tasks.ValueObjects;

namespace Apollo.Domain.Tasks.Models;

public record Reminder(ReminderId Id, QuartzJobId QuartzJobId, ReminderTime ReminderTime);
