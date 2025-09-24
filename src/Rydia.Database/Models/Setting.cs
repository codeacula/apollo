using System.ComponentModel.DataAnnotations;

namespace Rydia.Database.Models;

/// <summary>
/// Represents a configuration setting stored in the database
/// </summary>
public class Setting
{
    /// <summary>
    /// Unique identifier for the setting
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The setting key (must be from SettingKeys enum)
    /// </summary>
    [Required]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The setting value
    /// </summary>
    [Required]
    public string Value { get; set; } = string.Empty;
}