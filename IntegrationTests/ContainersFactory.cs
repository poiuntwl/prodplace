using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace IntegrationTests;

public class ContainersFactory : IAsyncLifetime
{
    public readonly MsSqlContainer IdentityDbContainer;
    public readonly PostgreSqlContainer CustomerDbContainer;
    public readonly RabbitMqContainer RabbitMqContainer;

    public ContainersFactory()
    {
        IdentityDbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-22.04")
            .WithCleanUp(true)
            .Build();

        CustomerDbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithCleanUp(true)
            .Build();

        RabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
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
            RabbitMqContainer.StartAsync());
    }

    public async Task DisposeAsync()
    {
        await IdentityDbContainer.DisposeAsync();
        await CustomerDbContainer.DisposeAsync();
        await RabbitMqContainer.DisposeAsync();
    }
}

[CollectionDefinition(nameof(ContainersFactoryCollectionDefinition))]
public class ContainersFactoryCollectionDefinition : ICollectionFixture<ContainersFactory>;