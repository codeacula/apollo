using Apollo.API.Controllers;

namespace Apollo.API.Tests.Controllers;

public class ApiControllerTests
{
    [Fact]
    public void Ping_ReturnsCorrectResponse()
    {
        var controller = new ApiController();

        var result = controller.Ping();

        Assert.Equal("pong", result);
    }

    [Fact]
    public void Ping_ReturnsNonEmptyString()
    {
        var controller = new ApiController();

        var result = controller.Ping();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Ping_ReturnsConsistentValue()
    {
        var controller = new ApiController();

        var result1 = controller.Ping();
        var result2 = controller.Ping();

        Assert.Equal(result1, result2);
    }
}
