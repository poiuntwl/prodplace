namespace CurrencyRatesService;

public static class CacheConstants
{
    private const string CurrencyCacheKeyPrefix = "currency_";

    public static string GetCurrencyCacheKey(string? code)
    {
        return $"{CurrencyCacheKeyPrefix}{code?.Trim()}";
    }
}