using System.ComponentModel.DataAnnotations;

namespace Apollo.Database.Models;

/// <summary>
/// Represents a configuration setting stored in the database
/// </summary>
public class Setting
{
    /// <summary>
    /// Unique identifier for the setting
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The setting key
    /// </summary>
    [Required]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The setting value
    /// </summary>
    [Required]
    public string Value { get; set; } = string.Empty;
}