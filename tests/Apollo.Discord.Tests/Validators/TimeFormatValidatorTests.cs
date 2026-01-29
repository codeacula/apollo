using System.Text.RegularExpressions;

namespace Apollo.Discord.Tests.Validators;

public partial class TimeFormatValidatorTests
{
  private static readonly Regex TimeFormatRegex = MyRegex();

  [Theory]
  [InlineData("00:00", true)]
  [InlineData("06:00", true)]
  [InlineData("12:00", true)]
  [InlineData("14:30", true)]
  [InlineData("23:59", true)]
  [InlineData("09:15", true)]
  [InlineData("18:45", true)]
  public void TimeFormatRegexWithValidTimeReturnsTrue(string time, bool expected)
  {
    // Act
    bool result = TimeFormatRegex.IsMatch(time);

    // Assert
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("24:00")]
  [InlineData("25:00")]
  [InlineData("12:60")]
  [InlineData("12:99")]
  [InlineData("1:00")]
  [InlineData("01:0")]
  [InlineData("1:0")]
  [InlineData("12")]
  [InlineData("12:")]
  [InlineData(":00")]
  [InlineData("12:00:00")]
  [InlineData("12.00")]
  [InlineData("12-00")]
  [InlineData("abc")]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData("12: 00")]
  [InlineData("12 :00")]
  [InlineData(" 12:00")]
  [InlineData("12:00 ")]
  public void TimeFormatRegexWithInvalidTimeReturnsFalse(string time)
  {
    // Act
    bool result = TimeFormatRegex.IsMatch(time);

    // Assert
    Assert.False(result);
  }

  [Theory]
  [InlineData("00:00")]
  [InlineData("01:00")]
  [InlineData("02:00")]
  [InlineData("03:00")]
  [InlineData("04:00")]
  [InlineData("05:00")]
  [InlineData("06:00")]
  [InlineData("07:00")]
  [InlineData("08:00")]
  [InlineData("09:00")]
  [InlineData("10:00")]
  [InlineData("11:00")]
  [InlineData("12:00")]
  [InlineData("13:00")]
  [InlineData("14:00")]
  [InlineData("15:00")]
  [InlineData("16:00")]
  [InlineData("17:00")]
  [InlineData("18:00")]
  [InlineData("19:00")]
  [InlineData("20:00")]
  [InlineData("21:00")]
  [InlineData("22:00")]
  [InlineData("23:00")]
  public void TimeFormatRegexWithAllValidHoursReturnsTrue(string time)
  {
    // Act
    bool result = TimeFormatRegex.IsMatch(time);

    // Assert
    Assert.True(result);
  }

  [Theory]
  [InlineData("00:00")]
  [InlineData("00:15")]
  [InlineData("00:30")]
  [InlineData("00:45")]
  [InlineData("00:59")]
  public void TimeFormatRegexWithAllValidMinutesReturnsTrue(string time)
  {
    // Act
    bool result = TimeFormatRegex.IsMatch(time);

    // Assert
    Assert.True(result);
  }

  [GeneratedRegex("^([0-1][0-9]|2[0-3]):[0-5][0-9]$", RegexOptions.Compiled)]
  private static partial Regex MyRegex();
}
