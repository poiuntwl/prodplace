using System.Text.RegularExpressions;
using IdentityService;
using IdentityService.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace IdentityServiceTests;

public partial class IdentityServiceFactory : WebApplicationFactory<IAppMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;
    private readonly IConfiguration _unitTestsConfiguration;
    private readonly RabbitMqContainer _rabbitMqContainer;

    public IdentityServiceFactory()
    {
        _unitTestsConfiguration = SetupConfig();
        var password = GetPasswordFromConnectionString();

        _dbContainer = new MsSqlBuilder()
            .WithPortBinding(5402, 1433)
            .WithImage("mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-22.04")
            .WithPassword(password)
            .WithCleanUp(true)
            .Build();

        // todo: we don't actually need this queue. Mock rbmq client and remove unnecessary container setup
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithPortBinding(5672, 5672)
            .WithPortBinding(15672, 15672)
            .WithUsername("admin")
            .WithPassword("password")
            .Build();
    }

    [GeneratedRegex(@"(?<=Password\=).+?(?=(;|$))", RegexOptions.IgnoreCase)]
    private static partial Regex PasswordMatchRegex();

    private string GetPasswordFromConnectionString()
    {
        var connectionString = _unitTestsConfiguration.GetConnectionString("TestIdentityConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new KeyNotFoundException("Missing connection string");
        }

        return PasswordMatchRegex().Match(connectionString).Value;
    }

    private static IConfiguration SetupConfig()
    {
        var unitTestsDir = Path.GetDirectoryName(typeof(IdentityServiceFactory).Assembly.Location)!;
        var unitTestsConfigPath = Path.Combine(unitTestsDir, "testsettings.json");
        var unitTestsConfiguration = new ConfigurationBuilder().SetBasePath(unitTestsDir)
            .AddJsonFile(unitTestsConfigPath, optional: false, reloadOnChange: true).Build();
        if (unitTestsConfiguration == null)
        {
            throw new FileNotFoundException("Missing configuration file");
        }

        return unitTestsConfiguration;
    }

    public async Task InitializeAsync()
    {
        var dbContainerStartTask = _dbContainer.StartAsync();
        var rabbitMqContainerStartTask = _rabbitMqContainer.StartAsync();

        await Task.WhenAll(dbContainerStartTask, rabbitMqContainerStartTask);
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
        await _rabbitMqContainer.DisposeAsync();
        await DisposeAsync();
    }
}

[CollectionDefinition(nameof(IdentityServiceCollectionDefinition))]
public class IdentityServiceCollectionDefinition : ICollectionFixture<IdentityServiceFactory>;