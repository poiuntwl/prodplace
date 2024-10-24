extern alias ProductsServiceSUT;
using Grpc.Core;
using IntegrationTests.HttpClients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using NSubstitute;
using ProductsServiceSUT::IdentityGrpc.Server;
using ProductsServiceSUT::ProductsService;
using ProductsServiceSUT::ProductsService.Data;
using Respawn;
using Testcontainers.MongoDb;
using Testcontainers.RabbitMq;

namespace IntegrationTests.Factories;

public class ProductServiceFactory : WebApplicationFactory<IAppMarker>, IAsyncLifetime
{
    private readonly MongoDbContainer _dbContainer;
    private MongoClient _mongoClient;
    private RabbitMqContainer _rabbitMqContainer;
    private Respawner _respawner = default!;
    private AsyncServiceScope _serviceScope;
    public IProductServiceHttpClient HttpClient = default!;
    public IServiceProvider ServiceProvider = default!;

    public ProductServiceFactory(ContainersFactory containersFactory)
    {
        _dbContainer = containersFactory.ProductDbContainer;
        _mongoClient = new MongoClient(_dbContainer.GetConnectionString());
        _rabbitMqContainer = containersFactory.RabbitMqContainer;
    }

    public async Task InitializeAsync()
    {
        _serviceScope = Services.CreateAsyncScope();
        ServiceProvider = _serviceScope.ServiceProvider;
        HttpClient = ServiceProvider.GetRequiredService<IProductServiceHttpClient>();

        var db = new MongoClient(_dbContainer.GetConnectionString()).GetDatabase("default");
        await db.CreateCollectionAsync("prices");
        await db.CreateCollectionAsync("product");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        HttpClient.Dispose();
        await _serviceScope.DisposeAsync();
        await DisposeAsync();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MongoDefaultConnection"] = _dbContainer.GetConnectionString()
            });
        });
        return base.CreateHost(builder);
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
            new AsyncUnaryCall<ValidateResponse>(Task.FromResult(new ValidateResponse
            {
                IsValid = true
            }), default, default, default, default));
        grpcMock.ValidateTokenAsync(Arg.Any<ValidateTokenRequest>(), Arg.Any<Metadata>(), Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>()).ReturnsForAnyArgs(
            new AsyncUnaryCall<ValidateResponse>(Task.FromResult(new ValidateResponse
            {
                IsValid = true
            }), default, default, default, default));
        s.AddSingleton(grpcMock);
    }
}