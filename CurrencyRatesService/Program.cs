using CurrencyRatesService;
using CurrencyRatesService.Models;

var builder = Host.CreateApplicationBuilder(args);
var s = builder.Services;
s.AddHostedService<Worker>();
s.Configure<CurrencyApiClientOptions>(x =>
{
    x.BaseUrl = "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/";
    x.UsdCurrencyRatesUrl = "currencies/usd.json";
});
s.AddHttpClient<CurrencyApiHttpClient>();

var host = builder.Build();
host.Run();