namespace Apollo.Domain.People.ValueObjects;

public readonly record struct DailyTaskCount
{
  public int Value { get; init; }

  public DailyTaskCount(int value)
  {
    Value = value;
  }

  public static bool TryParse(int count, out DailyTaskCount result, out string? error)
  {
    error = null;
    result = default;

    if (count < 1 || count > 20)
    {
      error = "Daily task count must be between 1 and 20";
      return false;
    }

    result = new DailyTaskCount(count);
    return true;
  }
}
