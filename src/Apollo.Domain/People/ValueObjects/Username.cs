using Apollo.Domain.Common.Enums;

namespace Apollo.Domain.People.ValueObjects;

public readonly record struct Username(string Value, Platform Platform)
{
  public bool IsValid => !string.IsNullOrWhiteSpace(Value);

  public static implicit operator string(Username username)
  {
    return username.Value;
  }
}
