using System.Text.Json;

using Apollo.AI.Models;

using FluentResults;

namespace Apollo.AI.Planning;

public sealed class ToolPlanParser
{
  private static readonly JsonSerializerOptions _options = new()
  {
    PropertyNameCaseInsensitive = true
  };

  public static Result<ToolPlan> Parse(string? content)
  {
    if (string.IsNullOrWhiteSpace(content))
    {
      return Result.Ok(new ToolPlan());
    }

    try
    {
      var plan = JsonSerializer.Deserialize<ToolPlan>(content, _options);
      return Result.Ok(plan ?? new ToolPlan());
    }
    catch (JsonException ex)
    {
      return Result.Fail<ToolPlan>($"Invalid tool plan JSON: {ex.Message}");
    }
  }
}
