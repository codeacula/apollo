using System.ComponentModel;

using Apollo.Core;
using Apollo.Core.People;
using Apollo.Domain.People.ValueObjects;

using Microsoft.SemanticKernel;

namespace Apollo.Application.People;

public class PersonPlugin(IPersonStore personStore, PersonConfig personConfig, PersonId personId)
{
  public const string PluginName = "Persons";
  [KernelFunction("set_timezone")]
  [Description("Sets the user's timezone for interpreting reminder times. Accepts IANA timezone IDs (e.g., 'America/New_York', 'Europe/London') or common abbreviations (EST, CST, MST, PST, GMT, BST, CET, JST, AEST). US timezones are preferred for ambiguous abbreviations.")]
  public async Task<string> SetTimeZoneAsync(
    [Description("The timezone ID or common abbreviation (e.g., 'America/Chicago', 'EST', 'CST', 'Pacific')")] string timezone)
  {
    try
    {
      if (!PersonTimeZoneId.TryParse(timezone, out var timeZoneId, out var error))
      {
        return $"Failed to set timezone: {error}";
      }

      var result = await personStore.SetTimeZoneAsync(personId, timeZoneId);

      if (result.IsFailed)
      {
        return $"Failed to set timezone: {result.GetErrorMessages()}";
      }

      var displayName = timeZoneId.GetDisplayName();
      return $"Successfully set your timezone to {displayName} ({timeZoneId.Value}).";
    }
    catch (Exception ex)
    {
      return $"Error setting timezone: {ex.Message}";
    }
  }

  [KernelFunction("get_timezone")]
  [Description("Gets the user's current timezone setting")]
  public async Task<string> GetTimeZoneAsync()
  {
    try
    {
      var personResult = await personStore.GetAsync(personId);

      if (personResult.IsFailed)
      {
        return $"Failed to retrieve timezone: {personResult.GetErrorMessages()}";
      }

      var person = personResult.Value;
      if (person.TimeZoneId is null)
      {
        var defaultDisplayName = TimeZoneInfo.FindSystemTimeZoneById(personConfig.DefaultTimeZoneId).DisplayName;
        return $"You are currently using the default timezone: {defaultDisplayName} ({personConfig.DefaultTimeZoneId}).";
      }

      var displayName = person.TimeZoneId.Value.GetDisplayName();
      return $"Your timezone is set to: {displayName} ({person.TimeZoneId.Value.Value}).";
    }
    catch (Exception ex)
    {
      return $"Error retrieving timezone: {ex.Message}";
    }
  }

  [KernelFunction("set_daily_task_count")]
  [Description("Sets how many tasks Apollo suggests in daily plans. Must be between 1 and 20.")]
  public async Task<string> SetDailyTaskCountAsync(
    [Description("The number of tasks to suggest per day (1-20)")] int count)
  {
    try
    {
      if (!DailyTaskCount.TryParse(count, out var dailyTaskCount, out var error))
      {
        return $"Failed to set daily task count: {error}";
      }

      var result = await personStore.SetDailyTaskCountAsync(personId, dailyTaskCount);

      if (result.IsFailed)
      {
        return $"Failed to set daily task count: {result.Errors.FirstOrDefault()?.Message}";
      }

      return $"Successfully set your daily task count to {count} tasks.";
    }
    catch (Exception ex)
    {
      return $"Error setting daily task count: {ex.Message}";
    }
  }

  [KernelFunction("get_daily_task_count")]
  [Description("Gets the current daily task count setting (how many tasks Apollo suggests per day)")]
  public async Task<string> GetDailyTaskCountAsync()
  {
    try
    {
      var personResult = await personStore.GetAsync(personId);

      if (personResult.IsFailed)
      {
        return $"Failed to retrieve daily task count: {personResult.Errors.FirstOrDefault()?.Message}";
      }

      var person = personResult.Value;
      if (person.DailyTaskCount is null)
      {
        return $"You are currently using the default daily task count: {personConfig.DefaultDailyTaskCount} tasks per day.";
      }

      return $"Your daily task count is set to: {person.DailyTaskCount.Value.Value} tasks per day.";
    }
    catch (Exception ex)
    {
      return $"Error retrieving daily task count: {ex.Message}";
    }
  }
}
