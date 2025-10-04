using Microsoft.Extensions.Options;
using Rydia.API.Controllers;
using Rydia.Core.Configuration;

namespace Rydia.API.Tests.Controllers;

public class ApiControllerTests
{
    private readonly ApiController _controller;

    public ApiControllerTests()
    {
        var settings = new RydiaSettings();
        var options = new TestOptions<RydiaSettings>(settings);
        _controller = new ApiController(options);
    }

    [Fact]
    public void Ping_ReturnsCorrectResponse()
    {
        var result = _controller.Ping();

        Assert.Equal("pong", result);
    }

    [Fact]
    public void Ping_ReturnsNonEmptyString()
    {
        var result = _controller.Ping();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Ping_ReturnsConsistentValue()
    {
        var result1 = _controller.Ping();
        var result2 = _controller.Ping();

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GetSettings_ReturnsRydiaSettings()
    {
        var result = _controller.GetSettings();

        Assert.NotNull(result);
        var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
        Assert.IsType<RydiaSettings>(okResult.Value);
    }

    private class TestOptions<T> : IOptions<T> where T : class
    {
        public TestOptions(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
