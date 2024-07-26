namespace CurrencyRatesService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly CurrencyApiHttpClient _httpClient;

    public Worker(ILogger<Worker> logger, CurrencyApiHttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
            }

            var r = await _httpClient.GetAllUsdCurrencyRatesAsync(ct);

            await Task.Delay(5000, ct);
        }
    }
}