using IdentityService;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace IntegrationTests;

[Collection(nameof(ContainersFactoryCollectionDefinition))]
public class IdentityServiceFactory : WebApplicationFactory<IAppMarker>, IAsyncLifetime
{
    public HttpClient HttpClient = default!;
    private readonly MsSqlContainer _dbContainer;
    private SqlConnection _sqlConnection = default!;
    private Respawner _respawner = default!;
    private RabbitMqContainer _rabbitMqContainer;

    public IdentityServiceFactory(ContainersFactory containersFactory)
    {
        _dbContainer = containersFactory.IdentityDbContainer;
        _rabbitMqContainer = containersFactory.RabbitMqContainer;
    }

    public async Task InitializeAsync()
    {
        HttpClient = CreateClient();
        await InitRespawner();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((_, s) =>
        {
            s.Remove(s.Single(x => x.ServiceType == typeof(DbContextOptions<AppDbContext>)));
            s.AddDbContext<AppDbContext>(y => { y.UseSqlServer(_dbContainer.GetConnectionString()); });

            s.Remove(s.Single(x => x.ServiceType == typeof(RabbitMqSettings)));
            s.AddSingleton(new RabbitMqSettings
            {
                QueueName = "rabbitmq",
                HostName = _rabbitMqContainer.Hostname,
                Port = int.Parse(_rabbitMqContainer.GetConnectionString().Split(":").Last().Split("/")[0]),
                UserName = "rabbitmq",
                Password = "rabbitmq"
            });
        });

        base.ConfigureWebHost(builder);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await ResetDbAsync();
        await _sqlConnection.DisposeAsync();
        await DisposeAsync();
    }

    private async Task InitRespawner()
    {
        _sqlConnection = new SqlConnection(_dbContainer.GetConnectionString());

        await _sqlConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_sqlConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = ["dbo"]
        });
    }

    private async Task ResetDbAsync()
    {
        await _respawner.ResetAsync(_sqlConnection);
    }
}

[CollectionDefinition(nameof(IdentityServiceCollectionDefinition))]
public class IdentityServiceCollectionDefinition : ICollectionFixture<IdentityServiceFactory>;