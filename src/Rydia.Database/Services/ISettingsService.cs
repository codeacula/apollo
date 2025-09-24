namespace Rydia.Database.Services;

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
    /// Sets a setting value
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <param name="value">The setting value</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SetSettingAsync(string key, string value);

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