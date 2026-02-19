using System.Globalization;

using Apollo.AI;
using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed class TimeParsingService(
  IFuzzyTimeParser fuzzyTimeParser,
  IApolloAIAgent aiAgent,
  TimeProvider timeProvider) : ITimeParsingService
{
  private static readonly string[] ExactFormats =
  [
    // ISO 8601 variants
    "yyyy-MM-ddTHH:mm:ss",
    "yyyy-MM-ddTHH:mm:ssZ",
    "yyyy-MM-ddTHH:mm:ss.fffffffZ",
    "yyyy-MM-ddTHH:mm:sszzz",
    "yyyy-MM-dd HH:mm:ss",
    "yyyy-MM-dd HH:mm",
    "yyyy-MM-dd",

    // Time-only formats
    "h:mm tt",
    "hh:mm tt",
    "HH:mm",
    "h tt",

    // US date formats
    "M/d/yyyy h:mm tt",
    "M/d/yyyy HH:mm",
    "M/d/yyyy",

    // Named month formats
    "MMM d",
    "MMM d, yyyy",
    "MMMM d",
    "MMMM d, yyyy",
    "MMM d yyyy",
    "MMMM d yyyy",
    "d MMM yyyy",
    "d MMMM yyyy"
  ];

  public async Task<Result<DateTime>> ParseTimeAsync(
    string input,
    string? userTimeZoneId = null,
    CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(input))
    {
      return Result.Fail<DateTime>("Time expression is empty or whitespace.");
    }

    var now = timeProvider.GetUtcNow().UtcDateTime;

    // Step 1: Try FuzzyTimeParser (natural language patterns)
    var fuzzyResult = fuzzyTimeParser.TryParseFuzzyTime(input, now);
    if (fuzzyResult.IsSuccess)
    {
      return Result.Ok(fuzzyResult.Value);
    }

    // Step 2: Try DateTime.TryParseExact with common formats
    if (DateTime.TryParseExact(input.Trim(), ExactFormats, CultureInfo.InvariantCulture,
      DateTimeStyles.None, out var exactParsed))
    {
      var utcExact = ConvertToUtc(exactParsed, userTimeZoneId);
      return Result.Ok(utcExact);
    }

    // Step 3: Try DateTime.TryParse with InvariantCulture as general fallback
    if (DateTime.TryParse(input.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None,
      out var parsed))
    {
      var utcParsed = ConvertToUtc(parsed, userTimeZoneId);
      return Result.Ok(utcParsed);
    }

    // Step 4: LLM fallback (last resort)
    return await ParseWithLlmAsync(input, userTimeZoneId, now, cancellationToken);
  }

  private async Task<Result<DateTime>> ParseWithLlmAsync(
    string input,
    string? userTimeZoneId,
    DateTime now,
    CancellationToken cancellationToken)
  {
    var currentDateTime = now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
    var timezone = userTimeZoneId ?? "UTC";

    var parser = await aiAgent
      .CreateTimeParsingRequestAsync(input, timezone, currentDateTime);
    var result = await parser.ExecuteAsync(cancellationToken);

    if (!result.Success || string.IsNullOrWhiteSpace(result.Content))
    {
      return Result.Fail<DateTime>(
        "Invalid time format. Supported formats include: 'in 10 minutes', 'tomorrow at 3pm', " +
        "'next Monday', 'tonight', 'at noon', 'end of day', or ISO 8601 (e.g., 2025-12-31T10:00:00).");
    }

    var content = result.Content.Trim();

    if (content.Equals("UNPARSEABLE", StringComparison.OrdinalIgnoreCase))
    {
      return Result.Fail<DateTime>(
        "Invalid time format. Supported formats include: 'in 10 minutes', 'tomorrow at 3pm', " +
        "'next Monday', 'tonight', 'at noon', 'end of day', or ISO 8601 (e.g., 2025-12-31T10:00:00).");
    }

    if (!DateTime.TryParse(content, CultureInfo.InvariantCulture, DateTimeStyles.None, out var llmParsed))
    {
      return Result.Fail<DateTime>(
        "Invalid time format. Supported formats include: 'in 10 minutes', 'tomorrow at 3pm', " +
        "'next Monday', 'tonight', 'at noon', 'end of day', or ISO 8601 (e.g., 2025-12-31T10:00:00).");
    }

    var utcResult = ConvertToUtc(llmParsed, userTimeZoneId);
    return Result.Ok(utcResult);
  }

  private static DateTime ConvertToUtc(DateTime parsedDate, string? userTimeZoneId)
  {
    return parsedDate.Kind switch
    {
      DateTimeKind.Utc => parsedDate,
      DateTimeKind.Local => parsedDate.ToUniversalTime(),
      // DateTimeKind.Unspecified: treat as user-local if timezone known, otherwise assume UTC
      _ => ConvertUnspecifiedToUtc(parsedDate, userTimeZoneId)
    };
  }

  private static DateTime ConvertUnspecifiedToUtc(DateTime parsedDate, string? userTimeZoneId)
  {
    if (string.IsNullOrWhiteSpace(userTimeZoneId))
    {
      return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
    }

    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);
    return TimeZoneInfo.ConvertTimeToUtc(parsedDate, timeZone);
  }
}
