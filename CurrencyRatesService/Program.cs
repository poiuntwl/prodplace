using CurrencyRatesService;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var s = builder.Services;
s.AddControllersWithViews();
s.AddHangfire(x => x
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

s.AddHangfireServer();

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

app.UseHangfireDashboard();

app.MapControllers();
app.MapHangfireDashboard();

RecurringJob.AddOrUpdate("myRecurringJob", () => new MyJobs().DoSomething(), "* * * * * *");

app.Run();