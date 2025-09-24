namespace Rydia.Database.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rydia.Database.Constants;
using Rydia.Database.Models;

/// <summary>
/// Service for managing configuration settings in the database
/// </summary>
public class SettingsService(RydiaDbContext context, ILogger<SettingsService> logger) : ISettingsService
{
    private readonly RydiaDbContext _context = context;
    private readonly ILogger<SettingsService> _logger = logger;

    public async Task<string?> GetSettingAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to get setting with null or empty key");
            return null;
        }

        if (!SettingKeys.IsValidKey(key))
        {
            _logger.LogWarning("Attempted to get setting with invalid key: {Key}", key);
            return null;
        }

        try
        {
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == key);

            return setting?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving setting with key: {Key}", key);
            return null;
        }
    }

    public async Task<bool> SetSettingAsync(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to set setting with null or empty key");
            return false;
        }

        if (!SettingKeys.IsValidKey(key))
        {
            _logger.LogWarning("Attempted to set setting with invalid key: {Key}", key);
            return false;
        }

        if (value == null)
        {
            _logger.LogWarning("Attempted to set setting with null value for key: {Key}", key);
            return false;
        }

        try
        {
            var existingSetting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (existingSetting != null)
            {
                existingSetting.Value = value;
                existingSetting.UpdatedAt = DateTime.UtcNow;
                _logger.LogInformation("Updated setting {Key}", key);
            }
            else
            {
                var newSetting = new Setting
                {
                    Key = key,
                    Value = value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Settings.Add(newSetting);
                _logger.LogInformation("Created new setting {Key}", key);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value for key: {Key}", key);
            return false;
        }
    }

    public async Task<Dictionary<string, string>> GetAllSettingsAsync()
    {
        try
        {
            var settings = await _context.Settings
                .Select(s => new { s.Key, s.Value })
                .ToDictionaryAsync(s => s.Key, s => s.Value);

            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all settings");
            return new Dictionary<string, string>();
        }
    }

    public async Task<bool> DeleteSettingAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to delete setting with null or empty key");
            return false;
        }

        try
        {
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting == null)
            {
                _logger.LogInformation("Setting not found for deletion: {Key}", key);
                return false;
            }

            _context.Settings.Remove(setting);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted setting {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting setting with key: {Key}", key);
            return false;
        }
    }

    public async Task<bool> SettingExistsAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        try
        {
            return await _context.Settings.AnyAsync(s => s.Key == key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if setting exists with key: {Key}", key);
            return false;
        }
    }
}