using System.Globalization;

using Apollo.Core;
using Apollo.Core.ToDos;
using Apollo.Domain.People.Models;

using FluentResults;

namespace Apollo.AI;

public sealed class ApolloReminderMessageGenerator(
  IApolloAIAgent apolloAIAgent,
  TimeProvider timeProvider
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

      var requestBuilder = apolloAIAgent
        .CreateReminderRequest(
          person.TimeZoneId.ToString() ?? "",
          timeProvider.GetUtcDateTime().ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
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
