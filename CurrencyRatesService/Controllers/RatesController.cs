using CurrencyRatesService.Exceptions;
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
            try
            {
                var rate = await ratesGetter.GetCurrencyRateAsync(code, ct);
                return Ok(new GetCurrencyRateByCodeResponse(rate));
            }
            catch (CurrencyRateNotAvailableException e)
            {
                return NotFound(new
                {
                    Message = "Currency code not available",
                });
            }
        }

        var rates = await ratesGetter.GetCurrencyRatesAsync(codesArray, ct);
        return Ok(new GetCurrencyRatesByCodesResponse(rates));
    }
}

public record GetCurrencyRateByCodeResponse(decimal Rate);

public record GetCurrencyRatesByCodesResponse(IDictionary<string, decimal?> Rates);