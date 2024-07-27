using CurrencyRatesService.Interfaces;

namespace CurrencyRatesService;

public interface IUpdateCurrencyRatesJob
{
    Task UpdateAsync(CancellationToken ct);
}

public class UpdateCurrencyRatesJob : IUpdateCurrencyRatesJob
{
    private readonly ICurrencyRatesUpdater _updater;

    public UpdateCurrencyRatesJob(ICurrencyRatesUpdater updater)
    {
        _updater = updater;
    }

    public async Task UpdateAsync(CancellationToken ct)
    {
        await _updater.UpdateAllCurrencyRates(ct);
    }
}