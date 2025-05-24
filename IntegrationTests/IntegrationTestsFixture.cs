using IntegrationTests.Factories;

namespace IntegrationTests;

public class IntegrationTestFixture
    : IAsyncLifetime
{
    private readonly PerseveranceFactory _perseveranceFactory;
    public ProductServiceFactory ProductServiceFactory;
    public IdentityServiceFactory IdentityServiceFactory { get; private set; } = null!;
    public AdminServiceFactory AdminServiceFactory { get; private set; } = null!;

    public IntegrationTestFixture(PerseveranceFactory perseveranceFactory)
    {
        _perseveranceFactory = perseveranceFactory;
    }

    public async ValueTask InitializeAsync()
    {
        IdentityServiceFactory = new IdentityServiceFactory(_perseveranceFactory);
        ProductServiceFactory = new ProductServiceFactory(_perseveranceFactory);
        await Task.WhenAll(IdentityServiceFactory.InitializeAsync().AsTask(),
            ProductServiceFactory.InitializeAsync().AsTask());

        AdminServiceFactory = new AdminServiceFactory(IdentityServiceFactory.GrpcHandler);
        await AdminServiceFactory.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await AdminServiceFactory.DisposeAsync();
        await IdentityServiceFactory.DisposeAsync();
    }
}