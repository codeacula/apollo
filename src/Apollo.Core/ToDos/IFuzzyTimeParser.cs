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
  /// <param name="referenceTimeUtc">The reference time to calculate relative times from (should be UTC)</param>
  /// <returns>A Result containing the parsed UTC DateTime if successful, or failure if the input is not a recognized fuzzy time format</returns>
  Result<DateTime> TryParseFuzzyTime(string input, DateTime referenceTimeUtc);
}
