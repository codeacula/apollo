using Apollo.Domain.Common.Enums;

namespace Apollo.Domain.People.ValueObjects;

public readonly record struct PersonId(Platform Platform, string ProviderId)
{
  private const char Separator = ':';

  public bool IsValid => !string.IsNullOrWhiteSpace(ProviderId);

  public string Value => $"{Platform}{Separator}{ProviderId}";

  public static bool TryParse(string value, out PersonId personId)
  {
    personId = default;

    if (string.IsNullOrWhiteSpace(value))
    {
      return false;
    }

    var separatorIndex = value.IndexOf(Separator);
    if (separatorIndex <= 0 || separatorIndex == value.Length - 1)
    {
      return false;
    }

    var platformPart = value[..separatorIndex];
    var providerIdPart = value[(separatorIndex + 1)..];

    if (!Enum.TryParse(platformPart, ignoreCase: true, out Platform platform))
    {
      return false;
    }

    if (string.IsNullOrWhiteSpace(providerIdPart))
    {
      return false;
    }

    personId = new(platform, providerIdPart);
    return true;
  }

  public static PersonId Parse(string value)
  {
    if (!TryParse(value, out var personId))
    {
      throw new FormatException($"Invalid person id value '{value}'.");
    }

    return personId;
  }

  public override string ToString()
  {
    return Value;
  }
}
