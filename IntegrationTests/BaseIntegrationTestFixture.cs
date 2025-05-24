using IntegrationTests.Factories;

namespace IntegrationTests;

public class BaseIntegrationTestFixture
    : IAsyncLifetime
{
    private readonly PerseveranceFactory _perseveranceFactory;
    public ProductServiceFactory ProductServiceFactory { get; private set; } = null!;
    public IdentityServiceFactory IdentityServiceFactory { get; private set; } = null!;
    public AdminServiceFactory AdminServiceFactory { get; private set; } = null!;
    public CustomerServiceFactory CustomerServiceFactory { get; private set; } = null!;

    public BaseIntegrationTestFixture(PerseveranceFactory perseveranceFactory)
    {
        _perseveranceFactory = perseveranceFactory;
    }

    public async ValueTask InitializeAsync()
    {
        IdentityServiceFactory = new IdentityServiceFactory(_perseveranceFactory);
        ProductServiceFactory = new ProductServiceFactory(_perseveranceFactory);
        CustomerServiceFactory = new CustomerServiceFactory(_perseveranceFactory);

        await Task.WhenAll(
            IdentityServiceFactory.InitializeAsync().AsTask(),
            ProductServiceFactory.InitializeAsync().AsTask(),
            CustomerServiceFactory.InitializeAsync().AsTask());

        AdminServiceFactory = new AdminServiceFactory(IdentityServiceFactory.GrpcHandler);
        await AdminServiceFactory.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await AdminServiceFactory.DisposeAsync();
        await IdentityServiceFactory.DisposeAsync();
        await ProductServiceFactory.DisposeAsync();
        await CustomerServiceFactory.DisposeAsync();
    }
}