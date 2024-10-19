using System.Text.RegularExpressions;
using IdentityService;
using IdentityService.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace IdentityServiceTests;

public partial class IdentityServiceFactory : WebApplicationFactory<IAppMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;
    private readonly IConfiguration _unitTestsConfiguration;

    public IdentityServiceFactory()
    {
        var unitTestsDir = Path.GetDirectoryName(typeof(IdentityServiceFactory).Assembly.Location)!;
        var unitTestsConfigPath = Path.Combine(unitTestsDir, "testsettings.json");
        _unitTestsConfiguration = new ConfigurationBuilder().SetBasePath(unitTestsDir)
            .AddJsonFile(unitTestsConfigPath, optional: false, reloadOnChange: true).Build();

        var connectionString = _unitTestsConfiguration.GetConnectionString("TestIdentityConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Missing connection string");
        }

        var password = GetPasswordFromConnectionString(connectionString);

        _dbContainer = new MsSqlBuilder()
            .WithPortBinding(5402, 1433)
            .WithImage("mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-22.04")
            .WithPassword(password)
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((_, s) =>
        {
            s.Remove(s.Single(x => x.ServiceType == typeof(DbContextOptions<AppDbContext>)));
            s.AddDbContext<AppDbContext>(y =>
            {
                y.UseSqlServer(_unitTestsConfiguration.GetConnectionString("TestIdentityConnection"));
            });
        });

        base.ConfigureWebHost(builder);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await DisposeAsync();
    }

    [GeneratedRegex(@"(?<=Password\=).+?(?=(;|$))")]
    private static partial Regex PasswordMatchRegex();

    private static string GetPasswordFromConnectionString(string connectionString)
    {
        return PasswordMatchRegex().Match(connectionString).Value;
    }
}