namespace Apollo.Domain.Users.ValueObjects;

public readonly record struct UserId(Guid Value)
{
  public static implicit operator Guid(UserId value)
  {
    return value.Value;
  }
}
