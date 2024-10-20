using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using MessagingTools;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using UserService;
using UserService.Data;

namespace IntegrationTests;

public class CustomerServiceFactory : WebApplicationFactory<IAppMarker>, IAsyncLifetime
{
    public HttpClient HttpClient = default!;
    private readonly PostgreSqlContainer _dbContainer;
    private NpgsqlConnection _sqlConnection = default!;
    private Respawner _respawner = default!;
    private RabbitMqContainer _rabbitMqContainer;

    public CustomerServiceFactory(ContainersFactory containersFactory)
    {
        _dbContainer = containersFactory.CustomerDbContainer;
        _rabbitMqContainer = containersFactory.RabbitMqContainer;
    }

    public async Task InitializeAsync()
    {
        HttpClient = CreateClient();
        await InitRespawnerAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((_, s) =>
        {
            s.Remove(s.Single(x => x.ServiceType == typeof(DbContextOptions<AppDbContext>)));
            s.AddDbContext<AppDbContext>(y => { y.UseNpgsql(_dbContainer.GetConnectionString()); });

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

    private async Task InitRespawnerAsync()
    {
        _sqlConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());

        await _sqlConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_sqlConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    private async Task ResetDbAsync()
    {
        await _respawner.ResetAsync(_sqlConnection);
    }
}

[CollectionDefinition(nameof(CustomerServiceCollectionDefinition))]
public class CustomerServiceCollectionDefinition : ICollectionFixture<CustomerServiceFactory>;