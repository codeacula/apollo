using Apollo.Domain.ToDos.ValueObjects;

namespace Apollo.Application.ToDos.Models;

public sealed record DailyPlan(
  IReadOnlyList<DailyPlanItem> SuggestedTasks,
  string SelectionRationale,
  int TotalActiveTodos
);

public sealed record DailyPlanItem(
  ToDoId Id,
  string Description,
  Priority Priority,
  Energy Energy,
  Interest Interest,
  DateTime? DueDate
);
