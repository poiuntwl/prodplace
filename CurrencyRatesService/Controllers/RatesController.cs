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
                return Ok(GetCurrencyRatesByCodesResponse.FromRate(code, rate));
            }
            catch (CurrencyRateNotAvailableException)
            {
                return NotFound(new Exception("Currency code not available"));
            }
        }

        var rates = await ratesGetter.GetCurrencyRatesAsync(codesArray, ct);
        return Ok(new GetCurrencyRatesByCodesResponse(rates));
    }
}

public record GetCurrencyRatesByCodesResponse
{
    public IDictionary<string, decimal?> Rates { get; set; }

    public GetCurrencyRatesByCodesResponse(IDictionary<string, decimal?> rates)
    {
        Rates = rates;
    }

    public static GetCurrencyRatesByCodesResponse FromRate(string code, decimal rate) =>
        new(new Dictionary<string, decimal?>
        {
            [code] = rate
        });
}