namespace Apollo.Domain.People.ValueObjects;

public readonly record struct PersonId(Guid Value)
{
  public static implicit operator Guid(PersonId value)
  {
    return value.Value;
  }
}
