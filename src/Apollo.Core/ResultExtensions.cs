using FluentResults;

namespace Apollo.Core;

/// <summary>
/// Extension methods for FluentResults Result types.
/// </summary>
public static class ResultExtensions
{
  /// <summary>
  /// Collects all error messages from a Result into a single string, separated by the specified separator.
  /// </summary>
  /// <param name="result">The result containing errors.</param>
  /// <param name="separator">The separator to use between error messages. Defaults to ", ".</param>
  /// <returns>A string containing all error messages, or an empty string if there are no errors.</returns>
  public static string GetErrorMessages(this ResultBase result, string separator = ", ")
  {
    return result.Errors.Count == 0
      ? string.Empty
      : string.Join(separator, result.Errors.Select(e => e.Message));
  }
}
