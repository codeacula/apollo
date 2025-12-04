namespace Apollo.Domain.Users.ValueObjects;

public readonly record struct Username(string Value)
{
  public bool IsValid => !string.IsNullOrWhiteSpace(Value);

  public static implicit operator string(Username username)
  {
    return username.Value;
  }

  public static implicit operator Username(string value)
  {
    return new Username(value);
  }
}
