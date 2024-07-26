namespace CurrencyRatesService.Models;

public class CurrencyApiClientOptions
{
    public required string BaseUrl { get; set; }
    public required string UsdCurrencyRatesUrl { get; set; }
}