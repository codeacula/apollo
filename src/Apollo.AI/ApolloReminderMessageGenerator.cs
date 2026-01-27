using System.Globalization;

using Apollo.Core;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.People.Models;

using FluentResults;

namespace Apollo.AI;

public sealed class ApolloReminderMessageGenerator(
  IApolloAIAgent apolloAIAgent,
  TimeProvider timeProvider,
  PersonConfig personConfig
) : IReminderMessageGenerator
{
  public async Task<Result<string>> GenerateReminderMessageAsync(
    Person person,
    IEnumerable<string> toDoDescriptions,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var taskList = string.Join(", ", toDoDescriptions);

      // Get the user's timezone or use default
      var timeZoneId = person.TimeZoneId?.Value ?? personConfig.DefaultTimeZoneId;
      var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

      // Convert UTC time to user's timezone
      var utcNow = timeProvider.GetUtcDateTime();
      var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZoneInfo);

      var requestBuilder = apolloAIAgent
        .CreateReminderRequest(
          timeZoneId,
          localTime.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
          taskList
        );

      var result = await requestBuilder.ExecuteAsync(cancellationToken);

      if (!result.Success)
      {
        return Result.Fail<string>($"Failed to generate reminder message: {result.ErrorMessage}");
      }

      // Remove any surrounding quotes the LLM might add
      return Result.Ok(result.Content.Trim().Trim('"'));
    }
    catch (Exception ex)
    {
      return Result.Fail<string>($"Failed to generate reminder message: {ex.Message}");
    }
  }
}
