using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ProductsService.Controllers;

[ApiController, Route("health")]
public class HealthcheckController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Check()
    {
        return Ok("healthy");
    }
}