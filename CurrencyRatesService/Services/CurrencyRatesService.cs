using CurrencyRatesService.Data;
using CurrencyRatesService.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRatesService.Services;

public interface ICurrencyRatesGetter
{
    Task<decimal> GetCurrencyRateAsync(string code, CancellationToken ct);
    Task<IDictionary<string, decimal?>> GetCurrencyRatesAsync(ICollection<string> codes, CancellationToken ct);
}

public class CurrencyRatesGetter : ICurrencyRatesGetter
{
    private readonly IRedisCacheService _cache;
    private readonly AppDbContext _dbContext;
    private readonly IUpdateCurrencyRatesJob _updateCurrencyRatesJob;

    public CurrencyRatesGetter(IRedisCacheService cache, AppDbContext dbContext,
        IUpdateCurrencyRatesJob updateCurrencyRatesJob)
    {
        _cache = cache;
        _dbContext = dbContext;
        _updateCurrencyRatesJob = updateCurrencyRatesJob;
    }

    public async Task<decimal> GetCurrencyRateAsync(string code, CancellationToken ct)
    {
        var trimmedCode = code.Trim();
        var result = await InnerGetCurrencyRateAsync(trimmedCode, ct);
        if (result != null)
        {
            return result.Value;
        }

        await _updateCurrencyRatesJob.UpdateAsync(ct);
        var finalResult = await InnerGetCurrencyRateAsync(trimmedCode, ct);
        if (finalResult == null)
        {
            throw new CurrencyRateNotAvailableException(trimmedCode);
        }

        return finalResult.Value;
    }

    private async Task<decimal?> InnerGetCurrencyRateAsync(string code, CancellationToken ct)
    {
        var key = CacheConstants.GetCurrencyCacheKey(code);
        if (await _cache.HasKeyAsync(key))
        {
            return await _cache.GetAsync<decimal>(key);
        }

        var currencyRate =
            await _dbContext.CurrencyExchangeRates.FirstOrDefaultAsync(x => x.CurrencyCode.Equals(code), ct);
        var exchangeRate = currencyRate?.ExchangeRate;
        if (exchangeRate != null)
        {
            await _cache.SetAsync(key, exchangeRate.Value, TimeSpan.FromHours(24));
        }

        return exchangeRate;
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