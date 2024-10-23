using IdentityService;
using IdentityService.Data;
using IntegrationTests.HttpClients;
using MassTransit;
using MessagingTools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace IntegrationTests.Factories;

public class IdentityServiceFactory : WebApplicationFactory<IAppMarker>, IAsyncLifetime
{
    public IIdentityServiceHttpClient HttpClient = default!;
    public IServiceProvider ServiceProvider;

    private AsyncServiceScope _serviceScope;
    private readonly MsSqlContainer _dbContainer;
    private SqlConnection _sqlConnection = default!;
    private Respawner _respawner = default!;
    private readonly RabbitMqContainer _rabbitMqContainer;

    public IdentityServiceFactory(ContainersFactory containersFactory)
    {
        _dbContainer = containersFactory.IdentityDbContainer;
        _rabbitMqContainer = containersFactory.RabbitMqContainer;
    }

    public async Task InitializeAsync()
    {
        await InitRespawner();
        _serviceScope = Services.CreateAsyncScope();
        ServiceProvider = _serviceScope.ServiceProvider;
        HttpClient = ServiceProvider.GetRequiredService<IIdentityServiceHttpClient>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((_, s) =>
        {
            s.AddHttpClient<IIdentityServiceHttpClient, IdentityServiceHttpClient>(y =>
                new IdentityServiceHttpClient(CreateClient()));

            s.Remove(s.Single(x => x.ServiceType == typeof(DbContextOptions<AppDbContext>)));
            s.AddDbContext<AppDbContext>(y =>
            {
                y.UseSqlServer(_dbContainer.GetConnectionString());
            });

            s.Remove(s.Single(x => x.ServiceType == typeof(RabbitMqSettings)));
            s.AddSingleton(new RabbitMqSettings
            {
                QueueName = "rabbitmq",
                HostName = _rabbitMqContainer.Hostname,
                Port = int.Parse(_rabbitMqContainer.GetConnectionString().Split(":").Last().Split("/")[0]),
                UserName = "rabbitmq",
                Password = "rabbitmq",
                ConnectionString = _rabbitMqContainer.GetConnectionString()
            });

            s.AddMassTransitTestHarness(x =>
            {
                x.AddConsumers(typeof(IAppMarker).Assembly);
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    var rabbitMqSettings = ctx.GetRequiredService<RabbitMqSettings>();
                    cfg.Host(new Uri(rabbitMqSettings.ConnectionString), y =>
                    {
                        y.Username(rabbitMqSettings.UserName);
                        y.Password(rabbitMqSettings.Password);
                    });

                    cfg.ConfigureEndpoints(ctx);
                });
            });
        });

        base.ConfigureWebHost(builder);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        HttpClient.Dispose();
        await ResetDbAsync();
        await _sqlConnection.DisposeAsync();
        await _serviceScope.DisposeAsync();
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