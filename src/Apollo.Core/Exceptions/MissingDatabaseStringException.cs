namespace Apollo.Core.Exceptions;

public sealed class MissingDatabaseStringException : Exception
{
  public string Key { get; }

  public MissingDatabaseStringException()
  {
    Key = "<<unknown>>";
  }

  public MissingDatabaseStringException(string key)
      : base($"Required database connection string '{key}' is missing or empty.")
  {
    Key = key;
  }

  public MissingDatabaseStringException(string key, Exception inner)
      : base($"Required database connection string '{key}' is missing or empty.", inner)
  {
    Key = key;
  }
}
