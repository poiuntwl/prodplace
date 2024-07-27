using Hangfire;

namespace CurrencyRatesService;

public static class HangfireJobsStarter
{
    public static void StartJobs()
    {
        RecurringJob.AddOrUpdate<IUpdateCurrencyRatesJob>(
            "updateRatesJob",
            x => x.UpdateAsync(CancellationToken.None),
            Cron.Daily);
    }
}