namespace Rydia.Controllers;

using Microsoft.AspNetCore.Mvc;
using Rydia.Database.Constants;
using Rydia.Database.Services;

/// <summary>
/// API Controller for managing Rydia configuration settings
/// </summary>
[ApiController]
[Route("/api/settings")]
public class SettingsController(ISettingsService settingsService) : ControllerBase
{
    private readonly ISettingsService _settingsService = settingsService;

    /// <summary>
    /// Get all available setting keys
    /// </summary>
    [HttpGet("keys")]
    public ActionResult<IReadOnlyList<string>> GetAvailableKeys()
    {
        return Ok(SettingKeys.AllKeys);
    }

    /// <summary>
    /// Get all settings
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Dictionary<string, string>>> GetAllSettings()
    {
        var settings = await _settingsService.GetAllSettingsAsync();
        return Ok(settings);
    }

    /// <summary>
    /// Get a specific setting by key
    /// </summary>
    [HttpGet("{key}")]
    public async Task<ActionResult<string>> GetSetting(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest("Setting key cannot be empty");
        }

        if (!SettingKeys.IsValidKey(key))
        {
            return BadRequest($"Invalid setting key: {key}. Valid keys are: {string.Join(", ", SettingKeys.AllKeys)}");
        }

        var value = await _settingsService.GetSettingAsync(key);
        if (value == null)
        {
            return NotFound($"Setting with key '{key}' not found");
        }

        return Ok(value);
    }

    /// <summary>
    /// Set or update a setting
    /// </summary>
    [HttpPut("{key}")]
    public async Task<ActionResult> SetSetting(string key, [FromBody] SetSettingRequest request)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest("Setting key cannot be empty");
        }

        if (!SettingKeys.IsValidKey(key))
        {
            return BadRequest($"Invalid setting key: {key}. Valid keys are: {string.Join(", ", SettingKeys.AllKeys)}");
        }

        if (request?.Value == null)
        {
            return BadRequest("Setting value cannot be null");
        }

        var success = await _settingsService.SetSettingAsync(key, request.Value);
        if (!success)
        {
            return StatusCode(500, "Failed to set setting");
        }

        return Ok(new { Message = "Setting updated successfully", Key = key, Value = request.Value });
    }

    /// <summary>
    /// Delete a setting
    /// </summary>
    [HttpDelete("{key}")]
    public async Task<ActionResult> DeleteSetting(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest("Setting key cannot be empty");
        }

        var success = await _settingsService.DeleteSettingAsync(key);
        if (!success)
        {
            return NotFound($"Setting with key '{key}' not found");
        }

        return Ok(new { Message = "Setting deleted successfully", Key = key });
    }

    /// <summary>
    /// Check if a setting exists
    /// </summary>
    [HttpHead("{key}")]
    public async Task<ActionResult> SettingExists(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest();
        }

        var exists = await _settingsService.SettingExistsAsync(key);
        return exists ? Ok() : NotFound();
    }
}

/// <summary>
/// Request model for setting a configuration value
/// </summary>
public class SetSettingRequest
{
    /// <summary>
    /// The setting value
    /// </summary>
    public required string Value { get; set; }
}