namespace Apollo.Domain.People.ValueObjects;

public readonly record struct DailyTaskCount
{
  public int Value { get; init; }

  public DailyTaskCount(int value)
  {
    if (value is < 1 or > 20)
    {
      throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 1 and 20");
    }

    Value = value;
  }

  public static bool TryParse(int count, out DailyTaskCount result, out string? error)
  {
    error = null;
    result = default;

    if (count is < 1 or > 20)
    {
      error = "Daily task count must be between 1 and 20";
      return false;
    }

    result = new DailyTaskCount(count);
    return true;
  }
}
