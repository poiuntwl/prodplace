using Testcontainers.MongoDb;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace IntegrationTests.Factories;

public class ContainersFactory : IAsyncLifetime
{
    public readonly MsSqlContainer IdentityDbContainer;
    public readonly PostgreSqlContainer CustomerDbContainer;
    public readonly MongoDbContainer ProductDbContainer;
    public readonly RabbitMqContainer RabbitMqContainer;

    public ContainersFactory()
    {
        const string sqlServerImageName = "mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-22.04";
        const string postgresImageName = "postgres:17-alpine";
        const string rabbitmqImageName = "rabbitmq:3-management";
        const string mongoImageName = "mongo:7.0.14";

        IdentityDbContainer = new MsSqlBuilder()
            .WithImage(sqlServerImageName)
            .WithCleanUp(true)
            .Build();

        CustomerDbContainer = new PostgreSqlBuilder()
            .WithImage(postgresImageName)
            .WithCleanUp(true)
            .Build();

        ProductDbContainer = new MongoDbBuilder()
            .WithImage(mongoImageName)
            .WithCleanUp(true)
            .Build();

        RabbitMqContainer = new RabbitMqBuilder()
            .WithImage(rabbitmqImageName)
            .WithPortBinding(15672, true)
            .WithPortBinding(5672, true)
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            IdentityDbContainer.StartAsync(),
            CustomerDbContainer.StartAsync(),
            ProductDbContainer.StartAsync(),
            RabbitMqContainer.StartAsync());
    }

    public async Task DisposeAsync()
    {
        // await IdentityDbContainer.DisposeAsync();
        // await CustomerDbContainer.DisposeAsync();
        // await ProductDbContainer.DisposeAsync();
        // await RabbitMqContainer.DisposeAsync();
    }
}