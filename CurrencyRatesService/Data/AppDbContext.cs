using CurrencyRatesService.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRatesService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<CurrencyExchangeRateModel> CurrencyExchangeRates { get; set; }
}