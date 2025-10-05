using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apollo.Core.Services;

/// <summary>
/// Interface for settings service operations
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets a setting value by key
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <returns>The setting value, or null if not found</returns>
    Task<string?> GetSettingAsync(string key);

    /// <summary>
    /// Gets a setting value as a boolean
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <param name="defaultValue">Default value if setting not found or invalid</param>
    /// <returns>The boolean value or default</returns>
    Task<bool> GetBooleanSettingAsync(string key, bool defaultValue = false);

    /// <summary>
    /// Gets a setting value as an integer
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <param name="defaultValue">Default value if setting not found or invalid</param>
    /// <returns>The integer value or default</returns>
    Task<int> GetIntegerSettingAsync(string key, int defaultValue = 0);

    /// <summary>
    /// Sets a setting value
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <param name="value">The setting value</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SetSettingAsync(string key, string value);

    /// <summary>
    /// Sets a boolean setting value
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <param name="value">The boolean value</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SetBooleanSettingAsync(string key, bool value);

    /// <summary>
    /// Sets an integer setting value
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <param name="value">The integer value</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SetIntegerSettingAsync(string key, int value);

    /// <summary>
    /// Gets all settings
    /// </summary>
    /// <returns>Dictionary of all settings</returns>
    Task<Dictionary<string, string>> GetAllSettingsAsync();

    /// <summary>
    /// Deletes a setting by key
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <returns>True if successful, false if not found</returns>
    Task<bool> DeleteSettingAsync(string key);

    /// <summary>
    /// Checks if a setting exists
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <returns>True if the setting exists, false otherwise</returns>
    Task<bool> SettingExistsAsync(string key);
}
