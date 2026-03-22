using System.Text.RegularExpressions;

namespace Apollo.Discord.Tests.Validators;

public partial class TimeFormatValidatorTests
{
  private static readonly Regex TimeFormatRegex = MyRegex();

  public static TheoryData<string> ValidTimeCases => new()
  {
    "00:00", "06:00", "12:00", "14:30", "23:59", "09:15", "18:45",
    "01:00", "02:00", "03:00", "04:00", "05:00", "07:00", "08:00",
    "10:00", "11:00", "13:00", "15:00", "16:00", "17:00", "19:00",
    "20:00", "21:00", "22:00", "00:15", "00:30", "00:45", "00:59"
  };

  [Theory]
  [MemberData(nameof(ValidTimeCases))]
  public void TimeFormatRegexWithValidTimeReturnsTrue(string time)
  {
    // Act
    bool result = TimeFormatRegex.IsMatch(time);

    // Assert
    Assert.True(result);
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

  [GeneratedRegex("^([0-1][0-9]|2[0-3]):[0-5][0-9]$", RegexOptions.Compiled)]
  private static partial Regex MyRegex();
}
