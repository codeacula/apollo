namespace Rydia.Database.Models;

using System.ComponentModel.DataAnnotations;

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
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The setting value
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// When the setting was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the setting was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}