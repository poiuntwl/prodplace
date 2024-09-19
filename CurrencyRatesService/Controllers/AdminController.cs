using Microsoft.AspNetCore.Mvc;

namespace CurrencyRatesService.Controllers;

[Route("/api/[controller]/[action]")]
public class AdminController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ForceUpdateRates([FromServices] IUpdateCurrencyRatesJob updaterJob,
        CancellationToken ct)
    {
        await updaterJob.UpdateAsync(ct);

        return Ok();
    }
}