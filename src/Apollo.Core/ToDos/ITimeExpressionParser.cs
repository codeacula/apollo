using FluentResults;

namespace Apollo.Core.ToDos;

/// <summary>
/// Parses a specific class of time expression strings into absolute UTC DateTime values.
/// Implementations are discovered automatically via <see cref="TimeExpressionParserAttribute"/>.
/// </summary>
public interface ITimeExpressionParser
{
  /// <summary>
  /// Attempts to parse <paramref name="input"/> into a UTC DateTime relative to
  /// <paramref name="referenceTimeUtc"/>.
  /// </summary>
  /// <param name="input">The raw time expression string.</param>
  /// <param name="referenceTimeUtc">The UTC "now" used for relative calculations.</param>
  /// <returns>
  /// <see cref="Result{T}.IsSuccess"/> when the expression is recognised and parsed;
  /// <see cref="Result{T}.IsFailed"/> when the expression is not handled by this parser.
  /// </returns>
  Result<DateTime> TryParse(string input, DateTime referenceTimeUtc);
}
