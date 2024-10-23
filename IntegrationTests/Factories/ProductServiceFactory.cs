extern alias ProductsServiceSUT;
using Grpc.Core;
using IntegrationTests.HttpClients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ProductsServiceSUT::IdentityGrpc.Server;
using ProductsServiceSUT::ProductsService;
using ProductsServiceSUT::ProductsService.Data;
using Respawn;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace IntegrationTests.Factories;

public class ProductServiceFactory : WebApplicationFactory<IAppMarker>, IAsyncLifetime
{
    public IProductServiceHttpClient HttpClient = default!;
    private AsyncServiceScope _serviceScope;
    public IServiceProvider ServiceProvider = default!;

    private readonly MsSqlContainer _dbContainer;
    private SqlConnection _sqlConnection = default!;
    private Respawner _respawner = default!;
    private readonly RabbitMqContainer _rabbitMqContainer;

    public ProductServiceFactory(ContainersFactory containersFactory)
    {
        _dbContainer = containersFactory.ProductDbContainer;
        _rabbitMqContainer = containersFactory.RabbitMqContainer;
    }

    public async Task InitializeAsync()
    {
        await InitRespawnerAsync();
        _serviceScope = Services.CreateAsyncScope();
        ServiceProvider = _serviceScope.ServiceProvider;
        HttpClient = ServiceProvider.GetRequiredService<IProductServiceHttpClient>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((_, s) =>
        {
            s.Remove(s.Single(x => x.ServiceType == typeof(DbContextOptions<AppDbContext>)));
            s.AddDbContext<AppDbContext>(y => { y.UseSqlServer(_dbContainer.GetConnectionString()); });
            s.AddHttpClient<IProductServiceHttpClient, ProductServiceHttpClient>(x =>
                new ProductServiceHttpClient(CreateClient()));

            MockGrpcDummy(s);
            /*
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
            */
        });

        base.ConfigureWebHost(builder);
    }

    private static void MockGrpcDummy(IServiceCollection s)
    {
        // todo: don't mock, make it work with identity service
        var grpcMock =
            Substitute.For<ProductsServiceSUT::IdentityGrpc.Server.IdentityService.IdentityServiceClient>();
        grpcMock.ValidateRolesAsync(Arg.Any<ValidateRolesRequest>(), Arg.Any<Metadata>(), Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>()).ReturnsForAnyArgs(
            new AsyncUnaryCall<ValidateResponse>(Task.FromResult<ValidateResponse>(new ValidateResponse
            {
                IsValid = true
            }), default, default, default, default));
        grpcMock.ValidateTokenAsync(Arg.Any<ValidateTokenRequest>(), Arg.Any<Metadata>(), Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>()).ReturnsForAnyArgs(
            new AsyncUnaryCall<ValidateResponse>(Task.FromResult<ValidateResponse>(new ValidateResponse
            {
                IsValid = true
            }), default, default, default, default));
        s.AddSingleton(grpcMock);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        HttpClient.Dispose();
        await ResetDbAsync();
        await _sqlConnection.DisposeAsync();
        await _serviceScope.DisposeAsync();
        await DisposeAsync();
    }

    private async Task InitRespawnerAsync()
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