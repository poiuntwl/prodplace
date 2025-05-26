using Testcontainers.Keycloak;
using Testcontainers.MongoDb;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace IntegrationTests.Factories;

public class PerseveranceFactory : IAsyncLifetime
{
    public readonly MsSqlContainer IdentityDbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-22.04")
        .Build();

    public readonly MongoDbContainer ProductDbContainer = new MongoDbBuilder()
        .WithImage("mongo:7.0.14")
        .WithCleanUp(true)
        .Build();

    public readonly PostgreSqlContainer CustomerDbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    public readonly KeycloakContainer KeyCloakContainer = new KeycloakBuilder()
        .WithImage("quay.io/keycloak/keycloak:26.0")
        .WithPortBinding(8080, true)
        .WithEnvironment("KEYCLOAK_ADMIN", "admin")
        .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
        .Build();

    public readonly RabbitMqContainer RabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .WithPortBinding(15672, true)
        .WithPortBinding(5672, true)
        .Build();

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(
            IdentityDbContainer.StartAsync(),
            ProductDbContainer.StartAsync(),
            CustomerDbContainer.StartAsync(),
            RabbitMqContainer.StartAsync(),
            KeyCloakContainer.StartAsync()
        );
    }

    public async ValueTask DisposeAsync()
    {
        // Dispose of all containers in parallel
        await Task.WhenAll(
            IdentityDbContainer.DisposeAsync().AsTask(),
            ProductDbContainer.DisposeAsync().AsTask(),
            CustomerDbContainer.DisposeAsync().AsTask(),
            RabbitMqContainer.DisposeAsync().AsTask(),
            KeyCloakContainer.DisposeAsync().AsTask()
        );
    }
}