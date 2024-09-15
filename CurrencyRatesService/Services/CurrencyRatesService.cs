using CurrencyRatesService.Data;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRatesService.Services;

public interface ICurrencyRatesGetter
{
    Task<decimal?> GetCurrencyRateAsync(string code, CancellationToken ct);
    Task<IDictionary<string, decimal?>> GetCurrencyRatesAsync(ICollection<string> codes, CancellationToken ct);
}

public class CurrencyRatesGetter : ICurrencyRatesGetter
{
    private readonly IRedisCacheService _cache;
    private readonly AppDbContext _dbContext;

    public CurrencyRatesGetter(IRedisCacheService cache, AppDbContext dbContext)
    {
        _cache = cache;
        _dbContext = dbContext;
    }

    public async Task<decimal?> GetCurrencyRateAsync(string code, CancellationToken ct)
    {
        var trimmedCode = code.Trim();
        var key = CacheConstants.GetCurrencyCacheKey(trimmedCode);
        if (await _cache.HasKeyAsync(key))
        {
            await _cache.GetAsync<decimal>(key);
        }

        var currencyRate =
            await _dbContext.CurrencyExchangeRates.FirstOrDefaultAsync(x => x.CurrencyCode.Equals(trimmedCode), ct);
        return currencyRate?.ExchangeRate;
    }

    public async Task<IDictionary<string, decimal?>> GetCurrencyRatesAsync(ICollection<string> codes,
        CancellationToken ct)
    {
        var resultDic = new Dictionary<string, decimal?>();
        foreach (var c in codes)
        {
            var key = CacheConstants.GetCurrencyCacheKey(c.Trim());
            var hasKey = await _cache.HasKeyAsync(key);
            if (hasKey == false)
            {
                continue;
            }

            var cacheResult = await _cache.GetAsync<decimal>(key);
            resultDic.Add(c, cacheResult);
        }

        var notInCache = codes.Except(resultDic.Keys).Select(x => x.Trim());
        var dbResults = _dbContext.CurrencyExchangeRates.Where(x => notInCache.Contains(x.CurrencyCode)).ToList();
        foreach (var r in dbResults)
        {
            resultDic.Add(r.CurrencyCode, r.ExchangeRate);
        }

        return resultDic;
    }
}