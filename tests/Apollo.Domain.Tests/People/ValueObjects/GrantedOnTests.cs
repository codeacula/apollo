using Apollo.Domain.People.ValueObjects;

namespace Apollo.Domain.Tests.People.ValueObjects;

public class GrantedOnTests
{
  [Fact]
  public void GrantedOnStoresValue()
  {
    // Arrange
    var date = DateTime.UtcNow;

    // Act
    var grantedOn = new GrantedOn(date);

    // Assert
    Assert.Equal(date, grantedOn.Value);
  }

  [Fact]
  public void GrantedOnEqualityWorksCorrectly()
  {
    // Arrange
    var date = DateTime.UtcNow;
    var grantedOn1 = new GrantedOn(date);
    var grantedOn2 = new GrantedOn(date);
    var grantedOn3 = new GrantedOn(date.AddMinutes(1));

    // Act & Assert
    Assert.Equal(grantedOn1, grantedOn2);
    Assert.NotEqual(grantedOn1, grantedOn3);
  }
}
