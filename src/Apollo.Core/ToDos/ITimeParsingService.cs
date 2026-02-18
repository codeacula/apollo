using FluentResults;

namespace Apollo.Core.ToDos;

/// <summary>
/// Consolidated time parsing service that implements a multi-strategy pipeline:
/// FuzzyTimeParser -> TryParseExact (common formats) -> TryParse (InvariantCulture) -> LLM fallback.
/// All results are returned in UTC.
/// </summary>
public interface ITimeParsingService
{
  /// <summary>
  /// Parses a natural language or formatted time expression into a UTC DateTime.
  /// </summary>
  /// <param name="input">The time expression to parse (e.g., "in 10 minutes", "tomorrow at 3pm", "2025-12-31T10:00:00")</param>
  /// <param name="userTimeZoneId">Optional IANA timezone ID for converting local times to UTC</param>
  /// <param name="cancellationToken">Optional cancellation token</param>
  /// <returns>A Result containing the parsed UTC DateTime if successful, or failure with an error message</returns>
  Task<Result<DateTime>> ParseTimeAsync(
    string input,
    string? userTimeZoneId = null,
    CancellationToken cancellationToken = default);
}
