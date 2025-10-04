namespace Rydia.API.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api")]
public class ApiController : ControllerBase
{
    [HttpGet("")]
    public string Ping() => "pong";
}
