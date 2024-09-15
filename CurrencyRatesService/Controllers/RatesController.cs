using CurrencyRatesService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyRatesService.Controllers;

[Route("/api")]
public class RatesController : ControllerBase
{
    [HttpGet("{codes}")]
    public async Task<IActionResult> GetByCode(
        string codes,
        [FromServices] ICurrencyRatesGetter ratesGetter,
        CancellationToken ct)
    {
        var codesArray = codes.Split(",");
        if (codesArray.Length == 1)
        {
            var code = codesArray[0];
            var rate = await ratesGetter.GetCurrencyRateAsync(code, ct);
            if (rate == null)
            {
                return NotFound();
            }

            return Ok(new GetCurrencyRateByCodeResponse(rate.Value));
        }

        var rates = await ratesGetter.GetCurrencyRatesAsync(codesArray, ct);
        return Ok(new GetCurrencyRatesByCodesResponse(rates));
    }
}

public record GetCurrencyRateByCodeResponse(decimal Rate);

public record GetCurrencyRatesByCodesResponse(IDictionary<string, decimal?> Rates);