using Apollo.Application.Configuration;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Apollo.API.Controllers;

/// <summary>
/// Handles system initialization and configuration endpoints.
/// GET /api/setup/status - Check initialization status.
/// POST /api/setup - Initial system setup (one-time only).
/// </summary>
[ApiController]
[Route("api/setup")]
public sealed class SetupController(IMediator mediator) : ControllerBase
{
  /// <summary>
  /// GET /api/setup/status - Returns the system initialization status.
  /// Indicates which subsystems (AI, Discord, SuperAdmin) are configured.
  /// </summary>
  [HttpGet("status")]
  public async Task<IActionResult> GetStatusAsync(CancellationToken cancellationToken = default)
  {
    var query = new GetInitializationStatusQuery();
    var result = await mediator.Send(query, cancellationToken);

    if (result.IsFailed)
    {
      return BadRequest(new { error = result.Errors.FirstOrDefault()?.Message });
    }

    var status = result.Value;
    return Ok(new
    {
      isInitialized = status.IsInitialized,
      isAiConfigured = status.IsAiConfigured,
      isDiscordConfigured = status.IsDiscordConfigured,
      isSuperAdminConfigured = status.IsSuperAdminConfigured,
    });
  }

  /// <summary>
  /// POST /api/setup - Performs initial system setup.
  /// Accepts AI config, Discord config, and SuperAdmin Discord user ID.
  /// Only allowed on first-time setup (system must not be initialized).
  /// </summary>
  [HttpPost]
  public async Task<IActionResult> PostSetupAsync(
    [FromBody] SetupRequest request,
    CancellationToken cancellationToken = default)
  {
    if (request == null)
    {
      return BadRequest(new { error = "Request body is required." });
    }

    // Check initialization status first
    var statusQuery = new GetInitializationStatusQuery();
    var statusResult = await mediator.Send(statusQuery, cancellationToken);

    if (statusResult.IsSuccess && statusResult.Value.IsInitialized)
    {
      return Conflict(new { error = "System is already initialized. Use dedicated update endpoints to modify configuration." });
    }

    try
    {
      // Execute AI configuration update if provided
      if (!string.IsNullOrWhiteSpace(request.AiModelId) || !string.IsNullOrWhiteSpace(request.AiEndpoint))
      {
        var aiCmd = new UpdateAiConfigurationCommand(request.AiModelId, request.AiEndpoint, request.AiApiKey);
        var aiResult = await mediator.Send(aiCmd, cancellationToken);

        if (aiResult.IsFailed)
        {
          return BadRequest(new { error = $"AI configuration failed: {aiResult.Errors.FirstOrDefault()?.Message}" });
        }
      }

      // Execute Discord configuration update if provided
      if (!string.IsNullOrWhiteSpace(request.DiscordToken) || !string.IsNullOrWhiteSpace(request.DiscordPublicKey))
      {
        var discordCmd = new UpdateDiscordConfigurationCommand(request.DiscordToken, request.DiscordPublicKey, request.DiscordBotName);
        var discordResult = await mediator.Send(discordCmd, cancellationToken);

        if (discordResult.IsFailed)
        {
          return BadRequest(new { error = $"Discord configuration failed: {discordResult.Errors.FirstOrDefault()?.Message}" });
        }
      }

      // Execute SuperAdmin configuration update if provided
      if (!string.IsNullOrWhiteSpace(request.SuperAdminDiscordUserId))
      {
        var adminCmd = new UpdateSuperAdminConfigurationCommand(request.SuperAdminDiscordUserId);
        var adminResult = await mediator.Send(adminCmd, cancellationToken);

        if (adminResult.IsFailed)
        {
          return BadRequest(new { error = $"SuperAdmin configuration failed: {adminResult.Errors.FirstOrDefault()?.Message}" });
        }
      }

      // Verify final status
      var finalStatusResult = await mediator.Send(statusQuery, cancellationToken);
      var finalStatus = finalStatusResult.IsSuccess ? finalStatusResult.Value : null;

      return Ok(new
      {
        message = "Setup completed successfully.",
        isInitialized = finalStatus?.IsInitialized ?? false,
        isAiConfigured = finalStatus?.IsAiConfigured ?? false,
        isDiscordConfigured = finalStatus?.IsDiscordConfigured ?? false,
        isSuperAdminConfigured = finalStatus?.IsSuperAdminConfigured ?? false,
      });
    }
    catch (Exception ex)
    {
      return BadRequest(new { error = $"Setup failed: {ex.Message}" });
    }
  }
}

/// <summary>
/// Request DTO for initial system setup.
/// </summary>
public sealed record SetupRequest
{
  public string? AiModelId { get; init; }
  public string? AiEndpoint { get; init; }
  public string? AiApiKey { get; init; }
  public string? DiscordToken { get; init; }
  public string? DiscordPublicKey { get; init; }
  public string? DiscordBotName { get; init; }
  public string? SuperAdminDiscordUserId { get; init; }
}
