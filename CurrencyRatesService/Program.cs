using System.Net;
using CurrencyRatesService;
using CurrencyRatesService.Data;
using CurrencyRatesService.Interfaces;
using CurrencyRatesService.Models;
using CurrencyRatesService.Services;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;

var builder = Host.CreateApplicationBuilder(args);
var s = builder.Services;
s.AddHostedService<Worker>();
s.AddDbContext<AppDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
s.Configure<CurrencyApiClientOptions>(x =>
{
    x.BaseUrl = "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/";
    x.UsdCurrencyRatesUrl = "currencies/usd.json";
});
s.AddHttpClient<CurrencyApiHttpClient>()
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(x => x.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));
s.AddTransient<ICurrencyRatesUpdater, CurrencyRatesUpdater>();

var host = builder.Build();
host.Run();