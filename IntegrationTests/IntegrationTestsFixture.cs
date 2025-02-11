using IntegrationTests.Factories;

namespace IntegrationTests;

public class IntegrationTestFixture
    : IAsyncLifetime
{
    public ContainersFactory ContainersFactory { get; } = new();
    public IdentityServiceFactory IdentityServiceFactory { get; private set; } = null!;
    public AdminServiceFactory AdminServiceFactory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await ContainersFactory.InitializeAsync();

        IdentityServiceFactory = new IdentityServiceFactory(ContainersFactory);
        await IdentityServiceFactory.InitializeAsync();

        AdminServiceFactory = new AdminServiceFactory(IdentityServiceFactory.GrpcHandler);
        await AdminServiceFactory.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await AdminServiceFactory.DisposeAsync();
        await IdentityServiceFactory.DisposeAsync();
        await ContainersFactory.DisposeAsync();
    }
}