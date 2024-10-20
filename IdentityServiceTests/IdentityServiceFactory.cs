using System.Data.Common;
using IdentityService;
using IdentityService.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace IdentityServiceTests;

public class IdentityServiceFactory : WebApplicationFactory<IAppMarker>, IAsyncLifetime
{
    public HttpClient HttpClient = default!;


    private readonly MsSqlContainer _dbContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    public IdentityServiceFactory()
    {
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-22.04")
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

    public async Task InitializeAsync()
    {
        var dbContainerStartTask = _dbContainer.StartAsync();
        var rabbitMqContainerStartTask = _rabbitMqContainer.StartAsync();

        await Task.WhenAll(dbContainerStartTask, rabbitMqContainerStartTask);
        HttpClient = CreateClient();
        await InitRespawner();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((_, s) =>
        {
            s.Remove(s.Single(x => x.ServiceType == typeof(DbContextOptions<AppDbContext>)));
            s.AddDbContext<AppDbContext>(y => { y.UseSqlServer(_dbContainer.GetConnectionString()); });
        });

        base.ConfigureWebHost(builder);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
        await DisposeAsync();
    }

    public async Task ResetDbAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    private async Task InitRespawner()
    {
        _dbConnection = new SqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = new[] { "dbo" }
        });
    }
}

[CollectionDefinition(nameof(IdentityServiceCollectionDefinition))]
public class IdentityServiceCollectionDefinition : ICollectionFixture<IdentityServiceFactory>;