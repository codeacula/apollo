using Apollo.Core.Configuration;

namespace Apollo.Core.Services;

/// <summary>
/// Interface for providing strongly-typed settings
/// </summary>
public interface ISettingsProvider
{
  /// <summary>
  /// Reloads settings from the database
  /// </summary>
  Task ReloadAsync();

  /// <summary>
  /// Gets the current settings
  /// </summary>
  ApolloSettings GetSettings();
}
