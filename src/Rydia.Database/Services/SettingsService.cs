using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rydia.Core.Constants;
using Rydia.Database.Models;

namespace Rydia.Database.Services;

/// <summary>
/// Service for managing configuration settings in the database
/// </summary>
public partial class SettingsService(RydiaDbContext context, ILogger<SettingsService> logger) : ISettingsService
{
    private readonly RydiaDbContext _context = context;
    private readonly ILogger<SettingsService> _logger = logger;

    public async Task<string?> GetSettingAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            LogNullOrEmptyKey(_logger);
            return null;
        }

        if (!SettingKeys.IsValidKey(key))
        {
            LogInvalidKey(_logger, key);
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
            LogErrorRetrievingSetting(_logger, ex, key);
            return null;
        }
    }

    public async Task<bool> GetBooleanSettingAsync(string key, bool defaultValue = false)
    {
        var value = await GetSettingAsync(key);
        if (value == null)
        {
            return defaultValue;
        }

        if (bool.TryParse(value, out bool result))
        {
            return result;
        }

        // Handle common string representations of booleans
        var lowerValue = value.ToLowerInvariant();
        return lowerValue is "1" or "yes" or "on" or "enabled" ? true : defaultValue;
    }

    public async Task<int> GetIntegerSettingAsync(string key, int defaultValue = 0)
    {
        var value = await GetSettingAsync(key);
        if (value == null)
        {
            return defaultValue;
        }

        return int.TryParse(value, out int result) ? result : defaultValue;
    }

    public async Task<bool> SetBooleanSettingAsync(string key, bool value)
    {
        return await SetSettingAsync(key, value.ToString().ToLowerInvariant());
    }

    public async Task<bool> SetIntegerSettingAsync(string key, int value)
    {
        return await SetSettingAsync(key, value.ToString());
    }

    public async Task<bool> SetSettingAsync(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            LogNullOrEmptyKey(_logger);
            return false;
        }

        if (!SettingKeys.IsValidKey(key))
        {
            LogInvalidKey(_logger, key);
            return false;
        }

        if (value == null)
        {
            LogNullValue(_logger, key);
            return false;
        }

        try
        {
            var existingSetting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (existingSetting != null)
            {
                var updatedSetting = existingSetting with { Value = value };
                _context.Settings.Remove(existingSetting);
                _context.Settings.Add(updatedSetting);
                LogSettingUpdated(_logger, key);
            }
            else
            {
                var newSetting = new Setting
                {
                    Key = key,
                    Value = value
                };

                _context.Settings.Add(newSetting);
                LogSettingCreated(_logger, key);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            LogErrorSettingSetting(_logger, ex, key);
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
            LogErrorRetrievingAllSettings(_logger, ex);
            return new Dictionary<string, string>();
        }
    }

    public async Task<bool> DeleteSettingAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            LogNullOrEmptyKey(_logger);
            return false;
        }

        try
        {
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting == null)
            {
                LogSettingNotFound(_logger, key);
                return false;
            }

            _context.Settings.Remove(setting);
            await _context.SaveChangesAsync();
            
            LogSettingDeleted(_logger, key);
            return true;
        }
        catch (Exception ex)
        {
            LogErrorDeletingSetting(_logger, ex, key);
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
            LogErrorCheckingSettingExists(_logger, ex, key);
            return false;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Attempted to access setting with null or empty key")]
    private static partial void LogNullOrEmptyKey(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Attempted to access setting with invalid key: {Key}")]
    private static partial void LogInvalidKey(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Attempted to set setting with null value for key: {Key}")]
    private static partial void LogNullValue(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Information, Message = "Updated setting {Key}")]
    private static partial void LogSettingUpdated(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Information, Message = "Created new setting {Key}")]
    private static partial void LogSettingCreated(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Information, Message = "Setting not found for deletion: {Key}")]
    private static partial void LogSettingNotFound(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleted setting {Key}")]
    private static partial void LogSettingDeleted(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving setting with key: {Key}")]
    private static partial void LogErrorRetrievingSetting(ILogger logger, Exception exception, string key);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error setting value for key: {Key}")]
    private static partial void LogErrorSettingSetting(ILogger logger, Exception exception, string key);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error retrieving all settings")]
    private static partial void LogErrorRetrievingAllSettings(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error deleting setting with key: {Key}")]
    private static partial void LogErrorDeletingSetting(ILogger logger, Exception exception, string key);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error checking if setting exists with key: {Key}")]
    private static partial void LogErrorCheckingSettingExists(ILogger logger, Exception exception, string key);
}