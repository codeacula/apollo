using Apollo.API.Dashboard;

using Microsoft.AspNetCore.Mvc;

namespace Apollo.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(
  IDashboardOverviewService dashboardOverviewService) : ControllerBase
{
  [HttpGet("overview")]
  public async Task<IActionResult> GetOverviewAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      return Ok(await dashboardOverviewService.GetOverviewAsync(cancellationToken));
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { error = ex.Message });
    }
  }
}
