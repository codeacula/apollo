using Microsoft.AspNetCore.Mvc;

namespace Rydia;

[ApiController]
[Route("/api")]
public class ApiController : ControllerBase
{
    [HttpGet("")]
    public string Ping() => "pong";
}
