namespace Apollo.Database.People.Events;

public sealed record PersonDailyTaskCountUpdatedEvent(int DailyTaskCount, DateTime UpdatedOn) : BaseEvent;
