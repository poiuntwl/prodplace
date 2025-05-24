using Testcontainers.Keycloak;
using Testcontainers.MongoDb;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace IntegrationTests.Factories;

public class PerseveranceFactory : IAsyncLifetime
{
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
        .WithCleanUp(true)
        .Build();

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(KeyCloakContainer.StartAsync(), RabbitMqContainer.StartAsync());
    }

    public async ValueTask DisposeAsync()
    {
        await KeyCloakContainer.DisposeAsync();
        await RabbitMqContainer.DisposeAsync();
    }
}