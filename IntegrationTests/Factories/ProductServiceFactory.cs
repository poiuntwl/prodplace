extern alias ProductsServiceSUT;
using System.Text.Json;
using Grpc.Core;
using IntegrationTests.HttpClients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
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

        var mongoClient = new MongoClient(_dbContainer.GetConnectionString());
        var db = mongoClient.GetDatabase("prodPlace");
        await db.CreateCollectionAsync("prices");
        await db.CreateCollectionAsync("products");

        await SeedProductsAsync(db);
    }

    private static async Task SeedProductsAsync(IMongoDatabase db)
    {
        var prodCollection = db.GetCollection<BsonDocument>("products");
        var products = new[]
        {
            new BsonDocument
            {
                { "name", "Laptop" },
                { "description", "High-performance laptop with 16GB RAM and 512GB SSD" },
                { "price", 1299.99 },
                { "customFields", JsonSerializer.Serialize(new { color = "Silver", weight = "1.8kg" }) }
            },
            new BsonDocument
            {
                { "name", "Smartphone" },
                { "description", "Latest model with 5G capability and triple camera setup" },
                { "price", 799.99 },
                { "customFields", JsonSerializer.Serialize(new { color = "Black", storage = "256GB" }) }
            },
            new BsonDocument
            {
                { "name", "Headphones" },
                { "description", "Noise-cancelling wireless headphones with 30-hour battery life" },
                { "price", 249.99 },
                { "customFields", JsonSerializer.Serialize(new { color = "White", type = "Over-ear" }) }
            }
        };
        await prodCollection.InsertManyAsync(products);
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
                ["ConnectionStrings:MongoDefaultConnection"] = _dbContainer.GetConnectionString(),
                ["ExpirationInSeconds"] = 120.ToString()
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
        s.AddSingleton(grpcMock);
    }
}