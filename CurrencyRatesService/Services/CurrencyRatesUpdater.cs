﻿using CurrencyRatesService.Data;
using CurrencyRatesService.Interfaces;
using CurrencyRatesService.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRatesService.Services;

public class CurrencyRatesUpdater : ICurrencyRatesUpdater
{
    private readonly AppDbContext _appDbContext;
    private readonly CurrencyApiHttpClient _httpClient;
    private readonly IRedisCacheService _cache;

    public CurrencyRatesUpdater(AppDbContext appDbContext, CurrencyApiHttpClient httpClient, IRedisCacheService cache)
    {
        _appDbContext = appDbContext;
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task UpdateAllCurrencyRates(CancellationToken ct)
    {
        var allCurrencyRates = await _httpClient.GetAllUsdCurrencyRatesAsync(ct);

        // Fetch all existing currency rates
        var existingRates = await _appDbContext.CurrencyExchangeRates
            .ToDictionaryAsync(r => r.CurrencyCode.Trim(), r => r, ct);

        // Configure EF Core for better performance in bulk operations
        _appDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        _appDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        // Save changes in batches
        const int batchSize = 1000;
        var count = 0;

        var lastUpdated = DateTime.UtcNow;
        foreach (var (code, newRate) in allCurrencyRates)
        {
            var trimmedCode = code.Trim();
            var cacheKey = CacheConstants.GetCurrencyCacheKey(trimmedCode);
            await _cache.SetAsync(cacheKey, newRate, TimeSpan.FromHours(24));

            if (existingRates.TryGetValue(trimmedCode, out var existingRate))
            {
                // Update existing rate
                existingRate.ExchangeRate = newRate;
                existingRate.LastUpdated = lastUpdated;
                _appDbContext.Update(existingRate);
            }
            else
            {
                // Add new rate
                _appDbContext.CurrencyExchangeRates.Add(new CurrencyExchangeRateModel
                {
                    CurrencyCode = code,
                    ExchangeRate = newRate,
                    LastUpdated = lastUpdated
                });
            }

            count++;

            // Save changes and clear tracker every batchSize entities
            if (count % batchSize != 0)
            {
                continue;
            }

            await _appDbContext.SaveChangesAsync(ct);
            _appDbContext.ChangeTracker.Clear();
        }

        // Save any remaining changes
        if (count % batchSize != 0)
        {
            await _appDbContext.SaveChangesAsync(ct);
        }

        // Reset EF Core configuration
        _appDbContext.ChangeTracker.AutoDetectChangesEnabled = true;
        _appDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
    }
}