using Apollo.Domain.Common.Enums;

using MPerson = Apollo.Domain.People.Models.Person;

namespace Apollo.Database.People;

public sealed record Person
{
  public Guid Id { get; init; }
  public required string Username { get; init; }
  public Platform Platform { get; init; }
  public bool HasAccess { get; init; }
  public DateTime CreatedOn { get; init; }
  public DateTime UpdatedOn { get; init; }

  public static explicit operator MPerson(Person person)
  {
    return new()
    {
      Id = new(person.Id),
      Username = new(person.Username, person.Platform),
      HasAccess = new(person.HasAccess),
      CreatedOn = new(person.CreatedOn),
      UpdatedOn = new(person.UpdatedOn)
    };
  }
}
