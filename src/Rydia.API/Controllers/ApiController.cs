namespace Rydia.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rydia.Core.Configuration;

[ApiController]
[Route("/api")]
public class ApiController : ControllerBase
{
    private readonly RydiaSettings _settings;

    public ApiController(IOptions<RydiaSettings> settings)
    {
        _settings = settings.Value;
    }

    [HttpGet("")]
    public string Ping() => "pong";

    /// <summary>
    /// Example endpoint demonstrating IOptions usage for settings
    /// </summary>
    [HttpGet("settings")]
    public ActionResult<RydiaSettings> GetSettings()
    {
        return Ok(_settings);
    }
}
