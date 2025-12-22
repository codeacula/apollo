using System.ComponentModel;
using System.Text.Json;

using Apollo.Core.People;
using Apollo.Domain.People.ValueObjects;

using Microsoft.SemanticKernel;

namespace Apollo.Application.People;

public class PersonPlugin(IPersonStore personStore, PersonConfig personConfig, PersonId personId)
{
  [KernelFunction("set_timezone")]
  [Description("Sets the user's timezone for interpreting reminder times. Accepts IANA timezone IDs (e.g., 'America/New_York', 'Europe/London') or common abbreviations (EST, CST, MST, PST, GMT, BST, CET, JST, AEST). US timezones are preferred for ambiguous abbreviations.")]
  public async Task<string> SetTimezoneAsync(
    [Description("The timezone ID or common abbreviation (e.g., 'America/Chicago', 'EST', 'CST', 'Pacific')")] string timezone)
  {
    try
    {
      if (!PersonTimeZoneId.TryParse(timezone, out var timeZoneId, out var error))
      {
        return JsonSerializer.Serialize(new { success = false, error });
      }

      var result = await personStore.SetTimezoneAsync(personId, timeZoneId);

      if (result.IsFailed)
      {
        return JsonSerializer.Serialize(new { success = false, error = string.Join(", ", result.Errors.Select(e => e.Message)) });
      }

      var displayName = timeZoneId.GetDisplayName();
      return JsonSerializer.Serialize(new
      {
        success = true,
        message = $"Timezone set to {displayName} ({timeZoneId.Value})",
        timeZoneId = timeZoneId.Value,
        displayName
      });
    }
    catch (Exception ex)
    {
      return JsonSerializer.Serialize(new { success = false, error = ex.Message });
    }
  }

  [KernelFunction("get_timezone")]
  [Description("Gets the user's current timezone setting")]
  public async Task<string> GetTimezoneAsync()
  {
    try
    {
      var personResult = await personStore.GetAsync(personId);

      if (personResult.IsFailed)
      {
        return JsonSerializer.Serialize(new { success = false, error = string.Join(", ", personResult.Errors.Select(e => e.Message)) });
      }

      var person = personResult.Value;
      if (person.TimeZoneId is null)
      {
        return JsonSerializer.Serialize(new
        {
          success = true,
          timeZoneId = personConfig.DefaultTimeZoneId,
          displayName = TimeZoneInfo.FindSystemTimeZoneById(personConfig.DefaultTimeZoneId).DisplayName,
          isDefault = true
        });
      }

      return JsonSerializer.Serialize(new
      {
        success = true,
        timeZoneId = person.TimeZoneId.Value.Value,
        displayName = person.TimeZoneId.Value.GetDisplayName(),
        isDefault = false
      });
    }
    catch (Exception ex)
    {
      return JsonSerializer.Serialize(new { success = false, error = ex.Message });
    }
  }
}
