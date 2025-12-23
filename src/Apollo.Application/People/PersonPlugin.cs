using System.ComponentModel;

using Apollo.Core.People;
using Apollo.Domain.People.ValueObjects;

using Microsoft.SemanticKernel;

namespace Apollo.Application.People;

public class PersonPlugin(IPersonStore personStore, PersonConfig personConfig, PersonId personId)
{
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
        var errors = string.Join(", ", result.Errors.Select(e => e.Message));
        return $"Failed to set timezone: {errors}";
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
        var errors = string.Join(", ", personResult.Errors.Select(e => e.Message));
        return $"Failed to retrieve timezone: {errors}";
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
}
