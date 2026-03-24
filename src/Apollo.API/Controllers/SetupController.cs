using Apollo.Application.Configuration;

using Apollo.Core;

using FluentResults;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace Apollo.API.Controllers;

/// <summary>
/// Handles system initialization and configuration endpoints.
/// GET /api/setup/status - Check initialization status.
/// POST /api/setup - Initial system setup (one-time only).
/// </summary>
/// <param name="mediator"></param>
[ApiController]
[Route("api/setup")]
public sealed class SetupController(IMediator mediator) : ControllerBase
{
  /// <summary>
  /// GET /api/setup/status - Returns the system initialization status.
  /// Indicates which subsystems (AI, Discord, SuperAdmin) are configured.
  /// </summary>
  /// <param name="cancellationToken"></param>
  [HttpGet("status")]
  public async Task<IActionResult> GetStatusAsync(CancellationToken cancellationToken = default)
  {
    var query = new GetInitializationStatusQuery();
    var result = await mediator.Send(query, cancellationToken);

    if (result.IsFailed)
    {
      return BadRequest(new { error = GetErrorMessage(result) });
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
  /// <param name="request"></param>
  /// <param name="cancellationToken"></param>
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

    var aiModelId = request.Ai?.ModelId ?? request.AiModelId;
    var aiEndpoint = request.Ai?.Endpoint ?? request.AiEndpoint;
    var aiApiKey = request.Ai?.ApiKey ?? request.AiApiKey;
    var discordToken = request.Discord?.Token ?? request.DiscordToken;
    var discordPublicKey = request.Discord?.PublicKey ?? request.DiscordPublicKey;
    var discordBotName = request.Discord?.BotName ?? request.DiscordBotName;
    var superAdminDiscordUserId = request.SuperAdmin?.DiscordUserId ?? request.SuperAdminDiscordUserId;

    var hasAiInput = !string.IsNullOrWhiteSpace(aiModelId)
      || !string.IsNullOrWhiteSpace(aiEndpoint)
      || !string.IsNullOrWhiteSpace(aiApiKey);
    var hasDiscordInput = !string.IsNullOrWhiteSpace(discordToken)
      || !string.IsNullOrWhiteSpace(discordPublicKey)
      || !string.IsNullOrWhiteSpace(discordBotName);
    var hasSuperAdminInput = !string.IsNullOrWhiteSpace(superAdminDiscordUserId);

    if (!hasAiInput && !hasDiscordInput && !hasSuperAdminInput)
    {
      return BadRequest(new { error = "At least one setup configuration section is required." });
    }

    try
    {
      // Execute AI configuration update if provided
      if (hasAiInput)
      {
        var aiCmd = new UpdateAiConfigurationCommand(aiModelId, aiEndpoint, aiApiKey);
        var aiResult = await mediator.Send(aiCmd, cancellationToken);

        if (aiResult.IsFailed)
        {
          return BadRequest(new { error = $"AI configuration failed: {GetErrorMessage(aiResult)}" });
        }
      }

      // Execute Discord configuration update if provided
      if (hasDiscordInput)
      {
        var discordCmd = new UpdateDiscordConfigurationCommand(discordToken, discordPublicKey, discordBotName);
        var discordResult = await mediator.Send(discordCmd, cancellationToken);

        if (discordResult.IsFailed)
        {
          return BadRequest(new { error = $"Discord configuration failed: {GetErrorMessage(discordResult)}" });
        }
      }

      // Execute SuperAdmin configuration update if provided
      if (hasSuperAdminInput)
      {
        var adminCmd = new UpdateSuperAdminConfigurationCommand(superAdminDiscordUserId);
        var adminResult = await mediator.Send(adminCmd, cancellationToken);

        if (adminResult.IsFailed)
        {
          return BadRequest(new { error = $"SuperAdmin configuration failed: {GetErrorMessage(adminResult)}" });
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

  private static string GetErrorMessage(ResultBase result)
  {
    return result.GetErrorMessages();
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
  public SetupAiRequest? Ai { get; init; }
  public string? DiscordToken { get; init; }
  public string? DiscordPublicKey { get; init; }
  public string? DiscordBotName { get; init; }
  public SetupDiscordRequest? Discord { get; init; }
  public string? SuperAdminDiscordUserId { get; init; }
  public SetupSuperAdminRequest? SuperAdmin { get; init; }
}

public sealed record SetupAiRequest
{
  public string? ModelId { get; init; }
  public string? Endpoint { get; init; }
  public string? ApiKey { get; init; }
}

public sealed record SetupDiscordRequest
{
  public string? Token { get; init; }
  public string? PublicKey { get; init; }
  public string? BotName { get; init; }
}

public sealed record SetupSuperAdminRequest
{
  public string? DiscordUserId { get; init; }
}
