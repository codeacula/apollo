using Apollo.Core.ToDos.Responses;
using Apollo.Domain.Common.Enums;

using NetCord.Rest;

namespace Apollo.Discord.Components;

public class DailyPlanComponent : ComponentContainerProperties
{
  public DailyPlanComponent(DailyPlanResponse dailyPlan)
  {
    AccentColor = Constants.Colors.ApolloGreen;

    var components = new List<IComponentContainerComponentProperties>
    {
      new TextDisplayProperties("# 📋 Your Daily Plan")
    };

    // Handle empty todos case
    if (dailyPlan.TotalActiveTodos == 0)
    {
      components.Add(new TextDisplayProperties(dailyPlan.SelectionRationale));
      Components = components;
      return;
    }

    // Show tasks in execution order
    for (int i = 0; i < dailyPlan.SuggestedTasks.Count; i++)
    {
      var task = dailyPlan.SuggestedTasks[i];

      var priorityEmoji = LevelToEmoji((Level)task.Priority);
      var energyEmoji = LevelToEmoji((Level)task.Energy);
      var interestEmoji = LevelToEmoji((Level)task.Interest);

      var dueDateText = task.DueDate.HasValue
        ? $" | 📅 <t:{new DateTimeOffset(task.DueDate.Value).ToUnixTimeSeconds()}:d>"
        : string.Empty;

      components.Add(new TextDisplayProperties(
        $"**{i + 1}.** {task.Description}\n" +
        $"└ {priorityEmoji} {energyEmoji} {interestEmoji}{dueDateText}"
      ));
    }

    // Add separator and rationale
    components.Add(new TextDisplayProperties("───────────────────────────────"));
    components.Add(new TextDisplayProperties($"💡 **Why these tasks?**\n{dailyPlan.SelectionRationale}"));

    // Add footer with task count
    components.Add(new TextDisplayProperties(
      $"📊 Showing {dailyPlan.SuggestedTasks.Count} of {dailyPlan.TotalActiveTodos} active todos"
    ));

    Components = components;
  }

  private static string LevelToEmoji(Level level) => level switch
  {
    Level.Blue => "🔵",
    Level.Green => "🟢",
    Level.Yellow => "🟡",
    Level.Red => "🔴",
    _ => "⚪"
  };
}
