using Microsoft.AspNetCore.Mvc;

namespace CurrencyRatesService.Controllers;

[Route("/api/[controller]")]
public class AdminController : ControllerBase
{
    [HttpGet("[action]")]
    public async Task<IActionResult> ForceUpdateRates([FromServices] IUpdateCurrencyRatesJob updaterJob,
        CancellationToken ct)
    {
        await updaterJob.UpdateAsync(ct);

        return Ok();
    }
}