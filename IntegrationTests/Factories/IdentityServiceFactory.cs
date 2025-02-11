using IdentityService;
using IdentityService.Data;
using IdentityService.Models;
using IntegrationTests.HttpClients;
using Keycloak.Net;
using Keycloak.Net.Models.Roles;
using MassTransit;
using MessagingTools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Respawn;
using Testcontainers.Keycloak;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using KeycloakConfiguration = IdentityService.Models.KeycloakConfiguration;

namespace IntegrationTests.Factories;

public class IdentityServiceFactory : WebApplicationFactory<IAppMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private readonly KeycloakContainer _keycloakContainer;
    private Respawner _respawner = default!;

    private AsyncServiceScope _serviceScope;
    private SqlConnection _sqlConnection = default!;
    public IIdentityServiceHttpClient HttpClient = default!;
    public IServiceProvider ServiceProvider = default!;
    public KeycloakClient KeycloakClient;

    public IdentityServiceFactory(ContainersFactory containersFactory)
    {
        _dbContainer = containersFactory.IdentityDbContainer;
        _rabbitMqContainer = containersFactory.RabbitMqContainer;
        _keycloakContainer = containersFactory.KeyCloakContainer;
    }

    public HttpMessageHandler GrpcHandler { get; set; }

    public async Task InitializeAsync()
    {
        await InitRespawner();
        _serviceScope = Services.CreateAsyncScope();
        ServiceProvider = _serviceScope.ServiceProvider;
        HttpClient = ServiceProvider.GetRequiredService<IIdentityServiceHttpClient>();
        var keycloakUrl = $"http://localhost:{_keycloakContainer.GetMappedPublicPort(8080)}";
        KeycloakClient = new KeycloakClient(keycloakUrl, "admin", "admin");

        await SetUpRolesAsync();

        GrpcHandler = Server.CreateHandler();
    }

    private async Task SetUpRolesAsync()
    {
        await KeycloakClient.CreateRoleAsync("master", new Role
        {
            Name = "user"
        });
        await KeycloakClient.CreateRoleAsync("master", new Role
        {
            Name = "manager"
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        HttpClient.Dispose();
        await ResetDbAsync();
        await _sqlConnection.DisposeAsync();
        await _serviceScope.DisposeAsync();
        await DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls("http://localhost:0");

        builder.ConfigureServices((_, s) =>
        {
            s.AddHttpClient<IIdentityServiceHttpClient, IdentityServiceHttpClient>(y =>
                new IdentityServiceHttpClient(CreateClient()));

            s.Remove(s.Single(x => x.ServiceType == typeof(DbContextOptions<AppDbContext>)));
            s.AddDbContext<AppDbContext>(y => { y.UseSqlServer(_dbContainer.GetConnectionString()); });

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

            var keycloakUrl = $"http://{_keycloakContainer.Hostname}:{_keycloakContainer.GetMappedPublicPort(8080)}";
            s.AddSingleton<IOptions<KeycloakConfiguration>>(x => Options.Create(new KeycloakConfiguration
            {
                ServerUrl = keycloakUrl,
                Realm = "master",
                AdminUsername = "admin",
                AdminPassword = "admin"
            }));

            s.AddGrpc();
        });

        base.ConfigureWebHost(builder);
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