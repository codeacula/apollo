namespace Apollo.Domain.People.ValueObjects;

public readonly record struct PersonTimeZoneId
{
  private static readonly Dictionary<string, string> CommonAbbreviations = new(StringComparer.OrdinalIgnoreCase)
  {
    // US Timezones (preferred for ambiguous cases)
    { "EST", "America/New_York" },
    { "EDT", "America/New_York" },
    { "Eastern", "America/New_York" },
    { "CST", "America/Chicago" },
    { "CDT", "America/Chicago" },
    { "Central", "America/Chicago" },
    { "MST", "America/Denver" },
    { "MDT", "America/Denver" },
    { "Mountain", "America/Denver" },
    { "PST", "America/Los_Angeles" },
    { "PDT", "America/Los_Angeles" },
    { "Pacific", "America/Los_Angeles" },
    // International Timezones
    { "GMT", "Europe/London" },
    { "UTC", "UTC" },
    { "BST", "Europe/London" },
    { "CET", "Europe/Paris" },
    { "CEST", "Europe/Paris" },
    { "JST", "Asia/Tokyo" },
    { "AEST", "Australia/Sydney" },
    { "AEDT", "Australia/Sydney" }
  };

  public string Value { get; init; }

  private PersonTimeZoneId(string value)
  {
    Value = value;
  }

  public static bool TryParse(string input, out PersonTimeZoneId timeZoneId, out string? error)
  {
    timeZoneId = default;
    error = null;

    if (string.IsNullOrWhiteSpace(input))
    {
      error = "Timezone cannot be empty";
      return false;
    }

    var trimmedInput = input.Trim();

    // Try to map common abbreviations
    var timeZoneIdString = CommonAbbreviations.TryGetValue(trimmedInput, out var mapped)
      ? mapped
      : trimmedInput;

    // Validate the timezone ID exists
    try
    {
      _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneIdString);
      timeZoneId = new PersonTimeZoneId(timeZoneIdString);
      return true;
    }
    catch (TimeZoneNotFoundException)
    {
      error = $"Timezone '{input}' is not recognized. Please use a valid IANA timezone ID (e.g., 'America/New_York') or common abbreviation (e.g., 'EST', 'CST', 'PST')";
      return false;
    }
    catch (InvalidTimeZoneException)
    {
      error = $"Timezone '{input}' is invalid";
      return false;
    }
  }

  public static implicit operator string(PersonTimeZoneId value)
  {
    return value.Value;
  }

  public TimeZoneInfo GetTimeZoneInfo()
  {
    return TimeZoneInfo.FindSystemTimeZoneById(Value);
  }

  public string GetDisplayName()
  {
    return GetTimeZoneInfo().DisplayName;
  }
}
