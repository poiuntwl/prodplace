using System.Text.Json;
using CurrencyRatesService.Models;
using Microsoft.Extensions.Options;

namespace CurrencyRatesService;

public class CurrencyApiHttpClient
{
    private readonly HttpClient _httpClient;
    private IOptions<CurrencyApiClientOptions> _options;

    public CurrencyApiHttpClient(HttpClient httpClient, IOptions<CurrencyApiClientOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options;
        _httpClient.BaseAddress = new Uri(_options.Value.BaseUrl);
    }

    public async Task<decimal> GetUsdToCurrencyRateAsync(CurrencyType currencyType, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync(_options.Value.UsdCurrencyRatesUrl, ct);
        response.EnsureSuccessStatusCode();
        var jsonDocument =
            await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var key = currencyType.ToString().ToLower();
        if (key == null)
        {
            throw new KeyNotFoundException(
                $"Currency type {currencyType.ToString()} does not have a currency definition.");
        }

        var root = jsonDocument.RootElement.GetProperty("usd");
        if (root.TryGetProperty(key, out var rate))
        {
            return rate.GetDecimal();
        }

        throw new KeyNotFoundException($"Currency rate for {key} not found in the response.");
    }

    public async Task<IDictionary<string, decimal>> GetAllUsdCurrencyRatesAsync(CancellationToken ct)
    {
        const string currenciesRootProperty = "usd";

        var response = await _httpClient.GetAsync(_options.Value.UsdCurrencyRatesUrl, ct);
        response.EnsureSuccessStatusCode();
        var jsonDocument =
            await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        var root = jsonDocument.RootElement.GetProperty(currenciesRootProperty);
        var pairs = root.EnumerateObject()
            .GroupBy(x => x.Name.ToLowerInvariant())
            .ToDictionary(x => x.Key, x => x.First().Value.GetDecimal(),
                StringComparer.OrdinalIgnoreCase);

        return pairs;
    }
}