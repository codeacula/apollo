using Apollo.AI;

using Microsoft.AspNetCore.Mvc;

namespace Apollo.API.Controllers;

[ApiController]
[Route("")]
public sealed class AiController(ApolloAIAgent agent, ILogger<AiController> logger) : ControllerBase
{
  private readonly ApolloAIAgent _agent = agent;
  private readonly ILogger<AiController> _logger = logger;

  [HttpPost("chat")]
  public async Task<ActionResult<ChatResponse>> ChatAsync([FromBody] ChatRequest request)
  {
    if (request is null || string.IsNullOrWhiteSpace(request.Message))
    {
      return BadRequest(new ProblemDetails
      {
        Title = "Invalid request",
        Detail = "Message is required."
      });
    }

    try
    {
      var response = await _agent.ChatAsync(request.Message);

      AiControllerLog.ChatCompleted(_logger);
      return Ok(new ChatResponse(response));
    }
    catch (Exception ex)
    {
      AiControllerLog.ChatFailed(_logger, ex.Message);
      return Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError, title: "AI chat failed");
    }
  }

  public sealed record ChatRequest(string Message);
  public sealed record ChatResponse(string Response);
}

internal static partial class AiControllerLog
{
  [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "AI chat completed successfully")]
  public static partial void ChatCompleted(ILogger logger);

  [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "AI chat failed: {Reason}")]
  public static partial void ChatFailed(ILogger logger, string Reason);
}

