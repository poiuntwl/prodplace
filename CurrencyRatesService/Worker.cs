using CurrencyRatesService.Interfaces;

namespace CurrencyRatesService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ICurrencyRatesUpdater _currencyRatesUpdater;

    public Worker(ILogger<Worker> logger, ICurrencyRatesUpdater currencyRatesUpdater)
    {
        _logger = logger;
        _currencyRatesUpdater = currencyRatesUpdater;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
            }

            await _currencyRatesUpdater.UpdateAllCurrencyRates(ct);

            await Task.Delay(TimeSpan.FromSeconds(60), ct);
        }
    }
}