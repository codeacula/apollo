using FluentResults;

namespace Apollo.Core.ToDos;

/// <summary>
/// Parses fuzzy/relative time expressions like "in 10 minutes" into absolute DateTime values.
/// </summary>
public interface IFuzzyTimeParser
{
  /// <summary>
  /// Attempts to parse a fuzzy time expression.
  /// </summary>
  /// <param name="input">The input string to parse (e.g., "in 10 minutes", "tomorrow")</param>
  /// <param name="referenceTimeUtc">The UTC reference time used for relative duration expressions.</param>
  /// <param name="userTimeZoneId">The user's timezone used to interpret wall-clock expressions like "tomorrow at 3pm".</param>
  /// <returns>A Result containing the parsed DateTime if successful, or failure if the input is not a recognized fuzzy time format.</returns>
  Result<DateTime> TryParseFuzzyTime(string input, DateTime referenceTimeUtc, string? userTimeZoneId = null);
}
