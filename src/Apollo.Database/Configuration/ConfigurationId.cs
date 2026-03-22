namespace Apollo.Database.Configuration;

public readonly record struct ConfigurationId(Guid Value)
{
  /// <summary>
  /// The singleton root ID for the application-wide configuration aggregate.
  /// </summary>
  public static Guid Root => new("00000000-0000-0000-0000-000000000001");
}
