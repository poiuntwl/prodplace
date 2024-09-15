using System.Net;
using CurrencyRatesService;
using CurrencyRatesService.Data;
using CurrencyRatesService.Interfaces;
using CurrencyRatesService.Models;
using CurrencyRatesService.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var s = builder.Services;
s.AddControllersWithViews();
s.AddDbContext<AppDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), x =>
{
    x.EnableRetryOnFailure();
}));
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
s.AddSingleton<IRedisCacheService, RedisCacheService>(x =>
{
    var connectionString = builder.Configuration.GetConnectionString("RedisCacheConnection");
    return new RedisCacheService(connectionString);
});
s.AddTransient<ICurrencyRatesGetter, CurrencyRatesGetter>();
s.AddTransient<ICurrencyRatesUpdater, CurrencyRatesUpdater>();
s.AddTransient<IUpdateCurrencyRatesJob, UpdateCurrencyRatesJob>();

s.AddHangfire(x => x
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

s.AddHangfireServer(x =>
{
    x.CancellationCheckInterval = TimeSpan.FromSeconds(5);
    x.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();


app.UseHangfireDashboard();
app.MapHangfireDashboard();

HangfireJobsStarter.StartJobs();

app.Run();