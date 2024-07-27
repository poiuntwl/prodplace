namespace CurrencyRatesService.Interfaces;

public interface ICurrencyRatesUpdater
{
    Task UpdateAllCurrencyRates(CancellationToken ct);
}