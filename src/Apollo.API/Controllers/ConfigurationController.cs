using Apollo.GRPC.Contracts;
using Apollo.GRPC.Service;
using Microsoft.AspNetCore.Mvc;

namespace Apollo.API.Controllers;

[ApiController]
[Route("api/configurations")]
public sealed class ConfigurationController(IApolloGrpcService grpcService) : ControllerBase
{
  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var result = await grpcService.GetAllConfigurationsAsync(new GetAllConfigurationsRequest());
    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
  }

  [HttpGet("{key}")]
  public async Task<IActionResult> Get(string key)
  {
    var result = await grpcService.GetConfigurationAsync(new GetConfigurationRequest { Key = key });
    return result.IsSuccess ? Ok(result.Value) : NotFound(result.Errors);
  }

  [HttpPost]
  public async Task<IActionResult> Set([FromBody] SetConfigurationRequest request)
  {
    var result = await grpcService.SetConfigurationAsync(request);
    return result.IsSuccess ? Ok() : BadRequest(result.Errors);
  }

  [HttpDelete("{key}")]
  public async Task<IActionResult> Delete(string key)
  {
    var result = await grpcService.DeleteConfigurationAsync(new DeleteConfigurationRequest { Key = key });
    return result.IsSuccess ? Ok() : BadRequest(result.Errors);
  }
}
